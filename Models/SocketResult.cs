namespace patter_pal.Models
{
    public enum SocketResultType
    {
        Error = -1,
        PartialSpeech = 0,
        SpeechResult = 1,
        PartialAnswer = 2,
        AnswerResult = 3,
    }

    public class SocketResult<T>
    {
        public SocketResult(T data, SocketResultType type)
        {
            Data = data;
            Type = type;
        }

        public T Data { get; set; }

        public SocketResultType Type { get; set; } 
    }
}
