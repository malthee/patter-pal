
using System.Text.Json.Serialization;

namespace patter_pal.Models
{
    public class OpenAiChatCompletionResponse
    {
        public const string DONE_RESPONSE = "data: [DONE]";

        public OpenAiChatCompletionResponse(string id, string @object, long created, string model, OpenAiChoice[] choices)
        {
            Id = id;
            Object = @object;
            Created = created;
            Model = model;
            Choices = choices;
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

        [JsonIgnore]
        public string? Content => Choices.FirstOrDefault()?.Delta?.Content;
    }

    public class OpenAiChoice
    {
    public OpenAiChoice(int index, OpenAiMessage delta, string finishReason)
    {
        Index = index;
        Delta = delta;
        FinishReason = finishReason;
    }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("delta")]
    public OpenAiMessage? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
    }

    // Not supported in streaming yet
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