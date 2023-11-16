using System.ComponentModel.DataAnnotations;

namespace patter_pal.Models
{
    /// <summary>
    /// Model for when the user starts a chat, continues the conversation.
    /// Also used when the server answers the user.
    /// </summary>
    public class ChatMessage
    {
        public ChatMessage(string text)
        {
            Text = text;
        }

        public string Text { get; set; }

        //[NotNull] TODO with context and validation
        //public Guid? ChatId { get; set; }

    }
}
