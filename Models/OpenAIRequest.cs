using patter_pal.Util;

namespace patter_pal.Models
{
    // Models used to communicate with OpenAi

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

        public string Model { get; set; }
        public List<OpenAiMessage> Messages { get; set; }
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }
        public double TopP { get; set; }
        public double FrequencyPenalty { get; set; }
        public double PresencePenalty { get; set; }
    }

    public class OpenAiMessage
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = "";
    }
}
