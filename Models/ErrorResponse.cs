namespace patter_pal.Models
{
    public class ErrorResponse
    {
        public enum ErrorCode
        {
            OpenAiLimitReached = 3,
            SpeechLimitReached = 4,
            NoSpeechRecognized = 5,
            // todo..
        }

        public ErrorResponse(string message, ErrorCode? code = null)
        {
            Message = message;
            Code = code;
        }

        public string Message { get; set; }

        public ErrorCode? Code { get; set; }
    }
}
