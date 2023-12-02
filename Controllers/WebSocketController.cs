using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using patter_pal.domain.Data;
using patter_pal.Logic;
using patter_pal.Logic.Interfaces;
using patter_pal.Models;
using patter_pal.Util;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using static patter_pal.Models.PronounciationMessageModel;

namespace patter_pal.Controllers
{
    /// <summary>
    /// Handles the speech recognition and pronounciation comming form the client.
    /// </summary>
    public class WebSocketController : ControllerBase
    {
        private readonly ILogger<WebSocketController> _logger;
        private readonly SpeechPronounciationService _speechPronounciationService;
        private readonly OpenAiService _openAiService;
        private readonly SpeechSynthesisService _speechSynthesisService;
        private readonly AuthService _authService;
        private readonly IPronounciationAnalyticsService _pronounciationAnalyticsService;
        private readonly IConversationService _conversationService;
        private readonly IUsageService _usageService;

        public WebSocketController(ILogger<WebSocketController> logger,
            SpeechPronounciationService speechPronounciationService,
            OpenAiService openAiService,
            SpeechSynthesisService speechSynthesisService,
            AuthService authService,
            IPronounciationAnalyticsService pronounciationAnalyticsService,
            IConversationService conversationService,
            IUsageService usageService)
        {
            _logger = logger;
            _speechPronounciationService = speechPronounciationService;
            _openAiService = openAiService;
            _speechSynthesisService = speechSynthesisService;
            _authService = authService;
            _pronounciationAnalyticsService = pronounciationAnalyticsService;
            _conversationService = conversationService;
            _usageService = usageService;
        }

        /// <summary>
        /// Starts a WebSocket connection and streams the audio to the speech recognition service.
        /// The result is then passed to the OpenAI service and the answer is returned to the client.
        /// Finally the answer is synthesized and passed to the client.<br/>
        /// May at any time abort the <see cref="WebSocket"/> if an error occurs.
        /// </summary>
        /// <param name="language">Language identifier</param>
        /// <param name="conversationId">Id of conversation if this is adding to an existing conversation</param>
        public async Task StartConversation(string language, string? conversationId = null)
        {
            string? userId = await _authService.GetUserId();
            if (!CheckForValidRequest(userId, language)) return;

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogDebug($"WebSocket started with language: {language}, conversationId: {conversationId}");

            // Check if user has requests remaining
            if (!await _usageService.HasUserRequestsRemainingAsync(userId!))
            {
                _logger.LogInformation($"User {userId} has no requests remaining, refusing request.");
                var error = new ErrorResponse("Sorry you have reached your chat limit. We have to limit requests to save costs as this is a student project. If you liked PatterPal please give us some feedback and we might give you a special access code!",
                                       ErrorResponse.ErrorCode.UsageLimitReached);
                await WebSocketHelper.SendTextWhenOpen(webSocket,
                    JsonSerializer.Serialize(new SocketResult<ErrorResponse>(error, SocketResultType.Error))
                );
                return;
            }

            // Azure Speech
            if (!ShouldContinueConversationFlow(webSocket)) return;
            var speechResult = await HandleSpeechRecognition(webSocket, language, userId!, conversationId);

            // Track requests
            await _usageService.IncrementUserRequestCounterAsync(userId!);

            // OpenAi
            if (!ShouldContinueConversationFlow(webSocket) || speechResult == null) return;
            (ConversationData conversation, SpeechRecognitionResult recognitionResult) = speechResult.Value;
            var conversationAnswer = await HandleOpenAiAnswer(webSocket, conversation, recognitionResult, language, userId!);

            // Synthesize answer
            if (!ShouldContinueConversationFlow(webSocket) || conversationAnswer == null) return;
            await _speechSynthesisService.SendSynthesizedText(webSocket, conversationAnswer, language);

            await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Finished", CancellationToken.None);
            _logger.LogDebug("WebSocket workflow finished");
        }

        private bool ShouldContinueConversationFlow(WebSocket ws) => ws.State == WebSocketState.Open;

        private bool CheckForValidRequest(string? userId, string language)
        {
            if (userId == null)
            {
                _logger.LogWarning($"User not logged in");
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return false;
            }

            if (!Regex.IsMatch(language, "^[a-zA-Z]{2}-[a-zA-Z1-9]{2,3}$"))
            {
                _logger.LogWarning($"Invalid language format: {language}");
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return false;
            }

            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.LogWarning("Request is not a WebSocket request");
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Performs the speech recognition, streams results to the client and adds the result to the database.
        /// Creates a new conversation if no conversationId is provided.
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="language"></param>
        /// <param name="userId"></param>
        /// <param name="initialConversationId"></param>
        /// <returns>The active <see cref="ConversationData"/> and the <see cref="SpeechRecognitionResult"/> if successful, null otherwise</returns>
        private async Task<(ConversationData conversation, SpeechRecognitionResult speechRecognition)?> HandleSpeechRecognition(WebSocket webSocket, string language, string userId, string? initialConversationId)
        {
            var reconitionResult = await _speechPronounciationService.StreamFromWebSocket(webSocket, language);
            if (reconitionResult == null)
            {
                _logger.LogWarning("Returned null from SpeechPronounciationService, aborting WebSocket");
                webSocket.Abort();
                return null;
            }

            // Create or get conversation from db
            var conversation = initialConversationId != null ?
                await _conversationService.GetConversationAndChatsAsync(userId, initialConversationId) :
                // Create a new Conversation with the title being the first 10 words of the first chat
                new ConversationData() { Title = string.Join(" ", reconitionResult.Text.Split(" ").Take(10)) };

            // Add chat to conversation
            var chatRequest = new ChatData(true, reconitionResult.Text, language);
            var pronounciation = PronunciationAssessmentResult.FromResult(reconitionResult);
            if (conversation == null
                // Add a new conversation if no conversationId was provided
                || (initialConversationId == null && !await _conversationService.AddConversationAsync(userId, conversation))
                || !await _conversationService.AddChatAsync(userId, conversation.Id!, chatRequest)
                || !await _pronounciationAnalyticsService.AddSpeechPronounciationResultDataAsync(userId, language, pronounciation))
            {
                await WebSocketHelper.SendTextWhenOpen(webSocket, JsonSerializer.Serialize(
                    new ErrorResponse("Could not find or create conversation. Please try again later.", ErrorResponse.ErrorCode.DatabaseError)
                ));
                _logger.LogError("Could not add Conversation or SpeechPronounciationResult to database.");
                webSocket.Abort();
                return null;
            }

            // Send result to user
            var result = new PronounciationMessageModel(chatRequest.Text,
                language,
                chatRequest.Id,
                conversation.Id!,
                pronounciation.AccuracyScore,
                pronounciation.FluencyScore,
                pronounciation.CompletenessScore,
                pronounciation.PronunciationScore,
                pronounciation.Words.Select(w => new Word(w.Word, w.AccuracyScore, w.ErrorType)).ToList()
            );
            await WebSocketHelper.SendTextWhenOpen(webSocket, JsonSerializer.Serialize(new SocketResult<PronounciationMessageModel>(result, SocketResultType.SpeechResult)));
            return (conversation, reconitionResult);
        }


        /// <summary>
        /// Requests answer form OpenAi, streams results to the client and adds the answer to the database.
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="conversation"></param>
        /// <param name="recognitionResult"></param>
        /// <param name="language"></param>
        /// <param name="userId"></param>
        /// <returns>The answer from OpenAi if successful, null otherwise</returns>
        private async Task<string?> HandleOpenAiAnswer(WebSocket webSocket, ConversationData conversation, SpeechRecognitionResult recognitionResult, string language, string userId)
        {
            var conversationAnswer = await _openAiService.StreamAndGenerateAnswer(webSocket, recognitionResult, conversation, language);
            if (conversationAnswer == null)
            {
                _logger.LogWarning("Returned null from OpenAiService, aborting WebSocket"); ;
                webSocket.Abort();
                return null;
            }

            var chatAnswer = new ChatData(false, conversationAnswer, language);

            // Add AI answer to database
            if (!await _conversationService.AddChatAsync(userId, conversation.Id, chatAnswer))
            {
                await WebSocketHelper.SendTextWhenOpen(webSocket, JsonSerializer.Serialize(
                     new ErrorResponse("Could not add answer to conversation. Please try again later.", ErrorResponse.ErrorCode.DatabaseError)
                 ));
                _logger.LogWarning($"Could not add to conversation ${conversation.Id} as ConversationService returned false.");
                webSocket.Abort();
                return null;
            }

            await WebSocketHelper.SendTextWhenOpen(webSocket,
                JsonSerializer.Serialize(new SocketResult<ChatMessageModel>(
                    new ChatMessageModel(chatAnswer.Text, chatAnswer.Language, chatAnswer.Id, conversation.Id, false), SocketResultType.AnswerResult))
                );
            return conversationAnswer;
        }
    }
}
