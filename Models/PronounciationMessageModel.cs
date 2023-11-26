
namespace patter_pal.Models
{
    /// <summary>
    /// A result of a speech pronounciation in form of a ChatMessage.
    /// </summary>
    public class PronounciationMessageModel : ChatMessageModel
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
            public string? ErrorType { get; set; } // Either None, Omission, Insertion or Mispronounciation
        }

        public PronounciationMessageModel(string text, string language, int chatId, string conversationId,
            double accuracyScore, double fluencyScore, double completenessScore, double pronunciationScore,
            List<Word> words)
        : base(text, language, chatId, conversationId)
        {
            Text = text;
            AccuracyScore = accuracyScore;
            FluencyScore = fluencyScore;
            CompletenessScore = completenessScore;
            PronunciationScore = pronunciationScore;
            Words = words;
            Language = language;
        }

        public double AccuracyScore { get; set; }
        public double FluencyScore { get; set; }
        public double CompletenessScore { get; set; }
        public double PronunciationScore { get; set; }
        public List<Word> Words { get; set; }
    }
}
