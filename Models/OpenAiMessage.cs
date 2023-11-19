using System.Text.Json.Serialization;

namespace patter_pal.Models
{
    public class OpenAiMessage
    {
        public const string ROLE_SYSTEM = "system";
        public const string ROLE_USER = "user";

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        public override string? ToString()
        {
            return $"OpenAiMessage - Role: {Role}, Content: {Content}";
        }
    }
}