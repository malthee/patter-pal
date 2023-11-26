using Newtonsoft.Json;

namespace patter_pal.domain.Data
{
    public class SpeechPronounciationResultData : ContainerItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }
        public string Language { get; set; } = string.Empty;
        public decimal AccuracyScore { get; set; }
        public decimal FluencyScore { get; set; }
        public decimal CompletenessScore { get; set; }
        public decimal PronounciationScore { get; set; }
        public List<WordData> Words { get; set; } = new();
    }
}
