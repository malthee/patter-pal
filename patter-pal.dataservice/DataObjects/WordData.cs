namespace patter_pal.dataservice.DataObjects
{
    public class WordData
    {
        public string Text { get; set; } = string.Empty;
        public decimal AccuracyScore { get; set; }
        public string ErrorType { get; set; } = string.Empty;
    }
}
