﻿namespace patter_pal.Models
{
    /// <summary>
    /// Model for conversation chat messages between user and the assistant.
    /// </summary>
    public class ChatMessageModel
    {
        public ChatMessageModel(string text, string language, int id, string conversationId, bool isUser)
        {
            Text = text;
            Language = language;
            Id = id;
            ConversationId = conversationId;
            IsUser = isUser;
            IsUser = isUser;
        }

        public string Text { get; set; }

        /// <summary>
        /// Language in the format of a language code like en-US
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Identifier of the message.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the conversation.
        /// </summary>
        public string ConversationId { get; set; }

        public bool IsUser { get; set; }
    }
}
