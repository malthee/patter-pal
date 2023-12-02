using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patter_pal.domain.Data
{
    public class UserData : ContainerItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int RequestCount { get; set; }
    }
}
