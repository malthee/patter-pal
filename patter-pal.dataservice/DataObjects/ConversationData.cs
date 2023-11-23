using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patter_pal.dataservice.DataObjects
{
    public class ConversationData : ContainerItem
    {

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        public List<ChatData> Data { get; set; } = new();

        public ConversationData()
        {
            
        }
        
        public static ConversationData Create(string userId, string title)
        {
            return new ConversationData
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                UserId = userId
            };
        }

        public void AddChatMessage(ChatData chat)
        {
            chat.Id = Data.Count;
            Data.Add(chat);
        }



        //TODO: add props
    }
}
