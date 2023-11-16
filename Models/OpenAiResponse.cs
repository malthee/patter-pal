
using System.Text.Json.Serialization;

namespace patter_pal.Models
{
    public class OpenAiChatCompletionResponse
    {
        public OpenAiChatCompletionResponse(string id, string @object, long created, string model, OpenAiChoice[] choices, OpenAiUsage usage)
        {
            Id = id;
            Object = @object;
            Created = created;
            Model = model;
            Choices = choices;
            Usage = usage;
        }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("choices")]
        public OpenAiChoice[] Choices { get; set; }

        [JsonPropertyName("usage")]
        public OpenAiUsage Usage { get; set; }

        [JsonIgnore]
        public string? LastAnswer => GetLastAnswer();

        private string GetLastAnswer()
        {
            if (Choices != null && Choices.Length > 0 && Choices[0]?.Message != null)
            {
                return Choices[0].Message.Content;
            }

            return null;
        }
    }

    public class OpenAiChoice
    {
    public OpenAiChoice(int index, OpenAiMessage message, string finishReason)
    {
        Index = index;
        Message = message;
        FinishReason = finishReason;
    }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public OpenAiMessage Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
    }

    public class OpenAiUsage
    {
        public OpenAiUsage(int promptTokens, int completionTokens, int totalTokens)
        {
            PromptTokens = promptTokens;
            CompletionTokens = completionTokens;
            TotalTokens = totalTokens;
        }

        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}