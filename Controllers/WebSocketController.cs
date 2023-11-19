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

        public WebSocketController(ILogger<HomeController> logger, SpeechPronounciationService speechPronounciationService, OpenAiService openAiService)
        {
            _logger = logger;
            _speechPronounciationService = speechPronounciationService;
            _openAiService = openAiService;
        }

        /// <summary>
        /// Starts a WebSocket connection and streams the audio to the speech recognition service.
        /// The result is then passed to the OpenAI service and the answer is returned to the client.
        /// </summary>
        /// <param name="language">Language identifier</param>
        /// <param name="chatId">Id of chat if this is adding to an existing conversation</param>
        public async Task StartConversation(string language, Guid? chatId = null)
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
            var reconitionResult = await _speechPronounciationService.StreamFromWebSocket(webSocket, language);    
            if(reconitionResult == null)
            { 
                _logger.LogDebug("Returned null from SpeechPronounciationService, aborting WebSocket");
                webSocket.Abort();
                return;
            }

            // OpenAi
            // TODO error handling
            var conversationAnswer = await _openAiService.StreamAndGenerateAnswer(webSocket, reconitionResult, language, chatId);
            if(conversationAnswer == null)
            {
                _logger.LogDebug("Returned null from OpenAiService, aborting WebSocket");
                webSocket.Abort();
                return;
            }
            var jsonResult = JsonSerializer.Serialize(conversationAnswer);
            await WebSocketHelper.SendTextWhenOpen(webSocket, jsonResult);

            await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Finished", CancellationToken.None);
            _logger.LogDebug("WebSocket workflow finished");
        }
    }
}
