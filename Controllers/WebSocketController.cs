using Microsoft.AspNetCore.Mvc;
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
        private readonly CosmosService _cosmosService;
        private readonly IConversationService _conversationService;

        public WebSocketController(ILogger<HomeController> logger, 
            SpeechPronounciationService speechPronounciationService, 
            OpenAiService openAiService, 
            SpeechSynthesisService speechSynthesisService, 
            AuthService authService, 
            CosmosService cosmosService, 
            IConversationService conversationService)
        {
            _logger = logger;
            _speechPronounciationService = speechPronounciationService;
            _openAiService = openAiService;
            _speechSynthesisService = speechSynthesisService;
            _authService = authService;
            _cosmosService = cosmosService;
            _conversationService = conversationService;
        }

        /// <summary>
        /// Starts a WebSocket connection and streams the audio to the speech recognition service.
        /// The result is then passed to the OpenAI service and the answer is returned to the client.
        /// </summary>
        /// <param name="language">Language identifier</param>
        /// <param name="conversationId">Id of conversation if this is adding to an existing conversation</param>
        public async Task StartConversation(string language, Guid? conversationId = null)
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
            /*
            // add new conversation if conversationId was null
            if (string.IsNullOrEmpty(conversationId))
            {
                ConversationData newConversation = new() { Title = $"New Conversation ({language})" };
                bool addConversationResult = await _conversationService.AddConversationAsync(userId, newConversation);
                if (!addConversationResult)
                {
                    _logger.LogWarning("Could not add conversation");
                    webSocket.Abort();
                    return;
                }
                conversationId = newConversation.Id;
            }

            bool addChatSuccess = await _conversationService.AddChatAsync(userId, conversationId, new ChatData(true, reconitionResult.Text, language));
            if (!addChatSuccess)
            {
                _logger.LogWarning("Could not add chat message");
                webSocket.Abort();
                return;
            }
            */

            var pronounciationResult = PronunciationAssessmentResult.FromResult(reconitionResult);
            var speechResultData = new SpeechPronounciationResultData
            {
                Language = language,
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                AccuracyScore = (decimal)pronounciationResult.AccuracyScore,
                FluencyScore = (decimal)pronounciationResult.FluencyScore,
                CompletenessScore = (decimal)pronounciationResult.CompletenessScore,
                PronounciationScore = (decimal)pronounciationResult.PronunciationScore,
                Words = new(pronounciationResult.Words.Select(w => new WordData() { AccuracyScore = (decimal)w.AccuracyScore, ErrorType = w.ErrorType, Text = w.Word }))

            };
            bool addSpeechResultSuccess = await _cosmosService.AddSpeechPronounciationResultDataAsync(userId, speechResultData);
            if (!addSpeechResultSuccess)
            {
                _logger.LogWarning("Could not add speech result");
                webSocket.Abort();
                return;
            }

            // OpenAi
            if (!ShouldContinueConversationFlow(webSocket)) return;
            var conversationAnswer = await _openAiService.StreamAndGenerateAnswer(webSocket, reconitionResult, language, conversationId);
            if (conversationAnswer == null)
            {
                _logger.LogWarning("Returned null from OpenAiService, aborting WebSocket");;
                webSocket.Abort();
                return;
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
