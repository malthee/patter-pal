using patter_pal.domain.Config;
using System.Text.Json.Serialization;

namespace patter_pal.Logic.Data
{
    public class OpenAiChatRequest
    {
        public OpenAiChatRequest(AppConfig config, List<OpenAiMessage> messages)
        {
            Model = config.OpenAiModel;
            Temperature = config.OpenAiTemperature;
            MaxTokens = config.OpenAiMaxOutputTokens;
            TopP = config.OpenAiTopP;
            FrequencyPenalty = config.OpenAiFrequencyPenalty;
            PresencePenalty = config.OpenAiPresencePenalty;
            Messages = messages;
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

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = true; // Stream the result, so we can get intermediate results
    }
}
