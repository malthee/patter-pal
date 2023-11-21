using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using patter_pal.Logic;
using patter_pal.Models;

namespace patter_pal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Policy = "LoggedInPolicy")]
    public class ConversationController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConversationService _conversationService;

        public ConversationController(ILogger<HomeController> logger, ConversationService conversationService)
        {
            _logger = logger;
            _conversationService = conversationService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ChatMessage>>> GetChatlog(Guid id)
        {
            // TODO
            return await _conversationService.GetChatlog(id);
        }
    }
}
