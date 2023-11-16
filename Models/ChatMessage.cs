using System.ComponentModel.DataAnnotations;

namespace patter_pal.Models
{
    /// <summary>
    /// Model for when the user starts a chat, continues the conversation.
    /// Also used when the server answers the user.
    /// </summary>
    public class ChatMessage
    {
        public ChatMessage(string text, string language)
        {
            Text = text;
            Language = language;
        }

        public string Text { get; set; }

        /// <summary>
        /// Language in the format of a language code like en-US
        /// </summary>
        public string Language { get; set; }

        //[NotNull] TODO with context and validation
        //public Guid? ChatId { get; set; }

    }
}
