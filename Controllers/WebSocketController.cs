using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using patter_pal.dataservice.Azure;
using patter_pal.dataservice.DataObjects;
using patter_pal.Logic;
using patter_pal.Logic.Interfaces;
using patter_pal.Models;
using patter_pal.Util;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace patter_pal.Controllers
{
    /// <summary>
    /// Handles the speech recognition and pronounciation comming form the client.
    /// </summary>
    public class WebSocketController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SpeechPronounciationService _speechPronounciationService;
        private readonly OpenAiService _openAiService;
        private readonly SpeechSynthesisService _speechSynthesisService;
        private readonly AuthService _authService;
        private readonly IPronounciationAnalyticsService _pronounciationAnalyticsService;
        private readonly IConversationService _conversationService;

        public WebSocketController(ILogger<HomeController> logger,
            SpeechPronounciationService speechPronounciationService,
            OpenAiService openAiService,
            SpeechSynthesisService speechSynthesisService,
            AuthService authService,
            IPronounciationAnalyticsService pronounciationAnalyticsService,
            IConversationService conversationService)
        {
            _logger = logger;
            _speechPronounciationService = speechPronounciationService;
            _openAiService = openAiService;
            _speechSynthesisService = speechSynthesisService;
            _authService = authService;
            _pronounciationAnalyticsService = pronounciationAnalyticsService;
            _conversationService = conversationService;
        }

        /// <summary>
        /// Starts a WebSocket connection and streams the audio to the speech recognition service.
        /// The result is then passed to the OpenAI service and the answer is returned to the client.
        /// </summary>
        /// <param name="language">Language identifier</param>
        /// <param name="conversationId">Id of conversation if this is adding to an existing conversation</param>
        public async Task StartConversation(string language, string? conversationId = null)
        {
            string? userId = await _authService.GetUserId();
            if (userId == null)
            {
                _logger.LogWarning($"User not logged in");
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            if (!Regex.IsMatch(language, "^[a-zA-Z]{2}-[a-zA-Z1-9]{2,3}$"))
            {
                _logger.LogWarning($"Invalid language format: {language}");
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.LogWarning("Request is not a WebSocket request");
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogDebug("WebSocket started");

            // Azure Speech
            if (!ShouldContinueConversationFlow(webSocket)) return;
            var reconitionResult = await _speechPronounciationService.StreamFromWebSocket(webSocket, language);
            if (reconitionResult == null)
            {
                _logger.LogWarning("Returned null from SpeechPronounciationService, aborting WebSocket");
                webSocket.Abort();
                return;
            }

            // Manage persistent conversation
            var conversation = conversationId != null ?
                await _conversationService.GetConversationAndChatsAsync(userId, conversationId) :
                // Create a new Conversation with the title being the first 10 words of the first chat
                new ConversationData() { Title = string.Join(" ", reconitionResult.Text.Split(" ").Take(10)) };
            if (conversation == null
                || !await _conversationService.AddConversationAsync(userId, conversation)
                || !await _conversationService.AddChatAsync(userId, conversation.Id, new ChatData(true, reconitionResult.Text, language))
                || !await _pronounciationAnalyticsService.AddSpeechPronounciationResultDataAsync(userId, language, PronunciationAssessmentResult.FromResult(reconitionResult)))
            {
                //WebSocketHelper.SendTextWhenOpen(webSocket, TODO
                _logger.LogError("Could not add Conversation or SpeechPronounciationResult to database.");
                webSocket.Abort();
                return;
            }

            // TODO check if conversation object is updated with added chat

            // OpenAi
            if (!ShouldContinueConversationFlow(webSocket)) return;
            var conversationAnswer = await _openAiService.StreamAndGenerateAnswer(webSocket, reconitionResult, conversation, language);
            if (conversationAnswer == null)
            {
                _logger.LogWarning("Returned null from OpenAiService, aborting WebSocket"); ;
                webSocket.Abort();
                return;
            }
            // TODO help asdfasdf Add AI answer to database
            if (!await _conversationService.AddChatAsync(userId, conversation.Id, new ChatData(false, conversationAnswer.Text, language)))
            {
                _logger.LogWarning($"Could not add to conversation ${conversationId} as ConversationService returned false. Will continue Websocket.");
            }

            // Synthesize answer
            if (!ShouldContinueConversationFlow(webSocket)) return;
            await _speechSynthesisService.SendSynthesizedText(webSocket, conversationAnswer.Text, language);

            await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Finished", CancellationToken.None);
            _logger.LogDebug("WebSocket workflow finished");
        }

        private bool ShouldContinueConversationFlow(WebSocket ws) => ws.State == WebSocketState.Open;
    }
}
