using patter_pal.Util;
using System.Text.Json.Serialization;

namespace patter_pal.Models
{
    public class OpenAiChatRequest
    {
        public OpenAiChatRequest(ChatMessage chatMessage, AppConfig config, List<OpenAiMessage>? history = null)
        {
            Model = config.OpenAiModel;
            Temperature = config.OpenAiTemperature;
            MaxTokens = config.OpenAiMaxTokens;
            TopP = config.OpenAiTopP;
            FrequencyPenalty = config.OpenAiFrequencyPenalty;
            PresencePenalty = config.OpenAiPresencePenalty;

            string languagePrompt = config.PromptForLanguage(chatMessage.Language);
            Messages = history ?? 
                new List<OpenAiMessage>(){new OpenAiMessage { Role=OpenAiMessage.ROLE_SYSTEM, Content = languagePrompt }}; // Only add prompt if no history as should already be present
            Messages.Add(new OpenAiMessage { Content = chatMessage.Text });
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
