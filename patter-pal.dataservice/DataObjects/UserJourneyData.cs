using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patter_pal.dataservice.DataObjects
{
    public class UserJourneyData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JourneyDetails { get; set; } = string.Empty;

        public UserJourneyData()
        {
                
        }

        //TODO: add props
    }
}
