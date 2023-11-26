namespace patter_pal.Models
{
    public class PronounciationAnalytics
    {
        public List<SpeechAssessmentData> SpeechAssessments { get; set; } = new();
        public List<WordStatistic>? BottomTenWords { get; set; }
        // TODO more word analytics like mispronounced words, insertions, deletions, etc.
    }

    public class SpeechAssessmentData
    {
        public DateTime Timestamp { get; set; }
        public string Language { get; set; } = string.Empty;
        public decimal AccuracyScore { get; set; }
        public decimal FluencyScore { get; set; }
        public decimal CompletenessScore { get; set; }
        public decimal PronounciationScore { get; set; }
    }

    public class WordStatistic
    {
        public string Text { get; set; } = string.Empty;
        public decimal AverageAccuracy { get; set; }
    }
}
