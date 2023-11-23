using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patter_pal.dataservice.DataObjects
{
    public interface ContainerItem
    {
        [JsonProperty(PropertyName = "id")]
        string Id { get; set; }
        string UserId { get; set; }
    }
}
