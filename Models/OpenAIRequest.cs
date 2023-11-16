using patter_pal.Util;

namespace patter_pal.Models
{
    public class OpenAiChatRequest
    {
        public OpenAiChatRequest(string userMessage, AppConfig config, List<OpenAiMessage> history)
        {
            Model = config.OpenAiModel;
            Temperature = config.OpenAiTemperature;
            MaxTokens = config.OpenAiMaxTokens;
            TopP = config.OpenAiTopP;
            FrequencyPenalty = config.OpenAiFrequencyPenalty;
            PresencePenalty = config.OpenAiPresencePenalty;

            Messages = history; 
            Messages.Add(new OpenAiMessage { Content = userMessage }); 
        }

        public OpenAiChatRequest(string userMessage, AppConfig config)
        {
            Model = config.OpenAiModel;
            Temperature = config.OpenAiTemperature;
            MaxTokens = config.OpenAiMaxTokens;
            TopP = config.OpenAiTopP;
            FrequencyPenalty = config.OpenAiFrequencyPenalty;
            PresencePenalty = config.OpenAiPresencePenalty;

            Messages = new List<OpenAiMessage>
            {
                new OpenAiMessage { Content = userMessage } 
            }; 
        }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public List<OpenAiMessage> Messages { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("top_p")]
        public double TopP { get; set; }

        [JsonPropertyName("frequency_penalty")]
        public double FrequencyPenalty { get; set; }

        [JsonPropertyName("presence_penalty")]
        public double PresencePenalty { get; set; }
    }
}
