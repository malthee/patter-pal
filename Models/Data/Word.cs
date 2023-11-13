namespace patter_pal.Models.Data
{
    public class Word
    {
        public string WordText { get; set; }
        public string ErrorType { get; set; }
        public double? AccuracyScore { get; set; }

        public Word(string wordText, string errorType)
        {
            WordText = wordText;
            ErrorType = errorType;
            AccuracyScore = null;
        }

        public Word(string wordText, string errorType, double accuracyScore) : this(wordText, errorType)
        {
            AccuracyScore = accuracyScore;
        }
    }
}
