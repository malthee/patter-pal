
using System.ComponentModel.DataAnnotations;

namespace patter_pal.Models
{
    public class ConversationModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string Title { get; set; }

        public ConversationModel(string id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}
