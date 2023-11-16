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
            return new List<ChatMessage> { new ChatMessage("ASDF", "de") };
        }

        public async Task<ChatMessage> GenerateAnswer(ChatMessage message)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appConfig.OpenAiKey); 
            var chatRequest = new OpenAiChatRequest(message, _appConfig);
            // Check for message token size, may reduce (forget) initial messages

            var response = await client.PostAsJsonAsync(_appConfig.OpenAiEndpoint, chatRequest);
            // TODO status code handling
            response.EnsureSuccessStatusCode();
            
            // TODO Log openaiusage in db

            var chatResponse = await response.Content.ReadFromJsonAsync<OpenAiChatCompletionResponse>();
            _logger.LogDebug($"Answer from OpenAI: {chatResponse?.LastAnswer}");

            if(chatResponse?.LastAnswer == null)
            {
                throw new Exception("No answer from OpenAI");
            }

            _logger.LogDebug($"Got answer from OpenAI: {chatResponse.LastAnswer}");
            return new ChatMessage(chatResponse.LastAnswer, message.Language); // Input language is output language
        }
    }
}
