using Microsoft.AspNetCore.Mvc;
using patter_pal.Logic.Interfaces;
using patter_pal.Models;

namespace patter_pal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class ConversationController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConversationService _conversationService;

        public ConversationController(ILogger<HomeController> logger, IConversationService conversationService)
        {
            _logger = logger;
            _conversationService = conversationService;
        }



    }
}
