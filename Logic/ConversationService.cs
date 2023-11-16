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
            return new List<ChatMessage> { new ChatMessage("ASDF") };
        }

        public async Task<ChatMessage> GenerateAnswer(ChatMessage message)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appConfig.OpenAiKey); 
            var chatRequest = new OpenAiChatRequest(message.Text, _appConfig);

            var response = await client.PostAsJsonAsync(_appConfig.OpenAiEndpoint, chatRequest);
            // TODO not working yet
            response.EnsureSuccessStatusCode();

            //var chatResponse = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>();
            var temp = await response.Content.ReadAsStringAsync();

            // TODO put answer in db
            return new ChatMessage(temp);
        }
    }
}
