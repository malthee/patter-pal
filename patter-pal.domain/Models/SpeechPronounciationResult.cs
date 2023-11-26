
namespace patter_pal.Models
{
    public class SpeechPronounciationResult
    {
        public class Word
        {
            public Word(string text, double accuracyScore, string errorType)
            {
                Text = text;
                AccuracyScore = accuracyScore;
                ErrorType = errorType;
            }

            public string Text { get; set; }
            public double AccuracyScore { get; set; }
            public string? ErrorType { get; set; }
        }

        public SpeechPronounciationResult(string text, string language, double accuracyScore, double fluencyScore, double completenessScore, double pronunciationScore, List<Word> words)
        {
            Text = text;
            AccuracyScore = accuracyScore;
            FluencyScore = fluencyScore;
            CompletenessScore = completenessScore;
            PronunciationScore = pronunciationScore;
            Words = words;
            Language = language;
        }

        public string Text { get; set; }
        public string Language { get; set; }
        public double AccuracyScore { get; set; }
        public double FluencyScore { get; set; }
        public double CompletenessScore { get; set; }
        public double PronunciationScore { get; set; }
        public List<Word> Words { get; set; }
    }
}
