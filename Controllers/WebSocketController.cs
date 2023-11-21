using Microsoft.AspNetCore.Mvc;
using patter_pal.Logic;
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

        public WebSocketController(ILogger<HomeController> logger, SpeechPronounciationService speechPronounciationService, OpenAiService openAiService, SpeechSynthesisService speechSynthesisService)
        {
            _logger = logger;
            _speechPronounciationService = speechPronounciationService;
            _openAiService = openAiService;
            _speechSynthesisService = speechSynthesisService;
        }

        /// <summary>
        /// Starts a WebSocket connection and streams the audio to the speech recognition service.
        /// The result is then passed to the OpenAI service and the answer is returned to the client.
        /// </summary>
        /// <param name="language">Language identifier</param>
        /// <param name="conversationId">Id of conversation if this is adding to an existing conversation</param>
        public async Task StartConversation(string language, Guid? conversationId = null)
        {
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
                // May move error messages into service
                await WebSocketHelper.SendTextWhenOpen(webSocket, JsonSerializer.Serialize(new ErrorResponse("Could not analyze speech. Please try again later.")));
                webSocket.Abort();
                return;
            }

            // OpenAi
            if (!ShouldContinueConversationFlow(webSocket)) return;
            var conversationAnswer = await _openAiService.StreamAndGenerateAnswer(webSocket, reconitionResult, language, conversationId);
            if (conversationAnswer == null)
            {
                _logger.LogWarning("Returned null from OpenAiService, aborting WebSocket");
                await WebSocketHelper.SendTextWhenOpen(webSocket, JsonSerializer.Serialize(new ErrorResponse("Could not get response. Please try again later.")));
                webSocket.Abort();
                return;
            }

            // Synthesize answer
            if (!ShouldContinueConversationFlow(webSocket)) return;
            await _speechSynthesisService.StreamSynthesizedText(webSocket, conversationAnswer.Text, language);

            await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Finished", CancellationToken.None);
            _logger.LogDebug("WebSocket workflow finished");
        }

        private bool ShouldContinueConversationFlow(WebSocket ws) => ws.State == WebSocketState.Open;
    }
}
