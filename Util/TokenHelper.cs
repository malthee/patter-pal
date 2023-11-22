
namespace patter_pal.Util
{
    public static class TokenHelper
    {
        // Conservative average characters per token for multi-language support
        private const double AverageCharactersPerToken = 2.5;

        // Estimates the token count for a given text
        public static int EstimateTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            return (int)Math.Ceiling(text.Length / AverageCharactersPerToken);
        }

        // Checks if the text exceeds a specified token limit
        public static bool ExceedsTokenLimit(string text, int maxTokenCount)
        {
            int tokenCount = EstimateTokenCount(text);
            return tokenCount > maxTokenCount;
        }

        // Estimates the total token count for an enumerable of strings
        public static int EstimateTotalTokenCount(IEnumerable<string> texts)
        {
            int totalTokenCount = 0;
            foreach (var text in texts)
            {
                totalTokenCount += EstimateTokenCount(text);
            }
            return totalTokenCount;
        }

        // Checks if the total token count for an enumerable of strings exceeds a specified token limit
        public static bool ExceedsTokenLimit(IEnumerable<string> texts, int maxTokenCount)
        {
            int totalTokenCount = EstimateTotalTokenCount(texts);
            return totalTokenCount > maxTokenCount;
        }
    }
}