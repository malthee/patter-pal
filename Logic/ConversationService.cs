using patter_pal.Controllers;
using patter_pal.Models;
using patter_pal.Util;
using System.Net.Http.Headers;

namespace patter_pal.Logic
{
    public class ConversationService
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppConfig _appConfig;

        public ConversationService(ILogger<HomeController> logger, AppConfig config, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _appConfig = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<ChatMessage>> GetChatlog(Guid id)
        {
            // TODO with db
            await Task.Delay(1000);
            return new List<ChatMessage> { new ChatMessage("ASDF", "de") };
        }

        // TODO get channels etc.
    }
}
