using Microsoft.CognitiveServices.Speech.PronunciationAssessment;

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

        public SpeechPronounciationResult(string text, PronunciationAssessmentResult par)
        {
            Text = text;
            AccuracyScore = par.AccuracyScore;
            FluencyScore = par.FluencyScore;
            CompletenessScore = par.CompletenessScore;
            PronunciationScore = par.PronunciationScore;
            Words = par.Words.Select(w => new Word(w.Word, w.AccuracyScore, w.ErrorType)).ToList();
        }

        public string Text { get; set; }
        public double AccuracyScore { get; set; }
        public double FluencyScore { get; set; }
        public double CompletenessScore { get; set; }
        public double PronunciationScore { get; set; }
        public List<Word> Words { get; set; }
    }
}
