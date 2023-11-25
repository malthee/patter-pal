namespace patter_pal.Models
{
    /// <summary>
    /// User friendly error response for the client.
    /// </summary>
    public class ErrorResponse
    {
        public enum ErrorCode
        {
            // Service errors may be caused by limitations or errors in the service we cannot control.
            OpenAiServiceError = 3, 
            SpeechServiceError = 4, 
            NoSpeechRecognized = 5,
            NoAnswerFound = 6,
            SynthesisServiceError = 7,
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
