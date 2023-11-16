using System.Text.Json.Serialization;

namespace patter_pal.Models
{
    public class OpenAiMessage
    {
        public const string ROLE_SYSTEM = "system";
        public const string ROLE_USER = "user";

        [JsonPropertyName("role")]
        public string Role { get; set; } = ROLE_USER;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}