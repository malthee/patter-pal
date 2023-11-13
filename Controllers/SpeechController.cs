using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using patter_pal.Logic;

namespace patter_pal.Controllers
{
    public class SpeechController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SpeechPronounciationService _speechPronounciationService;

        public SpeechController(ILogger<HomeController> logger, SpeechPronounciationService speechPronounciationService)
        {
            _logger = logger;
            _speechPronounciationService = speechPronounciationService;
        }

        public async Task Ws()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.LogDebug("SpeechController - WebSocket started");
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _speechPronounciationService.StartFromWebSocket(webSocket);    
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
