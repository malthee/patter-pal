namespace patter_pal.domain.Data
{
    public class ChatData
    {
        public int Id { get; set; }

        /// <summary>
        /// If the Chat is from the user or the assistant.
        /// </summary>
        public bool IsUser { get; set; }
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Language in the format of a language code like en-US
        /// </summary>
        public string Language { get; set; } = string.Empty;

        public ChatData(bool isUser, string text, string language)
        {
            IsUser = isUser;
            Text = text;
            Language = language;
        }
    }
}
