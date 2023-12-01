namespace patter_pal.Models
{
    public class PronounciationAnalyticsModel
    {
        public List<SpeechAssessmentData> SpeechAssessments { get; set; } = new();
        public List<WordStatistic>? BottomTenWords { get; set; }
        // Could add more stats in the future like words most mispronounced, etc.
        public string ChartUnit { get; set; } = string.Empty;
        public string ChartDisplayFormat { get; set; } = string.Empty;
        public string ChartDisplayFormatType { get; set; } = string.Empty;
    }

    public class SpeechAssessmentData
    {
        public DateTime Timestamp { get; set; }
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
