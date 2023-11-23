using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patter_pal.dataservice.DataObjects
{
    public class ChatConversationData : ContainerItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        public List<ChatMessageData> Data { get; set; } = new();

        public ChatConversationData()
        {
                
        }

        public void AddChatMessage(bool isUser, string text, string language)
        {
            Data.Add(new ChatMessageData(Data.Count, isUser, text, language));
        }

        public static ChatConversationData NewConversation(string userId, string title)
        {
            return new ChatConversationData
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                UserId = userId
            };
        }

        //TODO: add props
    }
}
