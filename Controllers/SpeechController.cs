using Microsoft.AspNetCore.Mvc;
using patter_pal.Logic;
using System.Text.RegularExpressions;

namespace patter_pal.Controllers
{
    /// <summary>
    /// Handles the speech recognition and pronounciation comming form the client.
    /// </summary>
    public class SpeechController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SpeechPronounciationService _speechPronounciationService;

        public SpeechController(ILogger<HomeController> logger, SpeechPronounciationService speechPronounciationService)
        {
            _logger = logger;
            _speechPronounciationService = speechPronounciationService;
        }

        public async Task RecognizeWs(string language, Guid? chatId = null)
        {
            if (!Regex.IsMatch(language, "^[a-zA-Z]{2}-[a-zA-Z]{2}$"))
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

            _logger.LogDebug("SpeechController - WebSocket started");
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _speechPronounciationService.StartFromWebSocket(webSocket, language);    
        }
    }
}
