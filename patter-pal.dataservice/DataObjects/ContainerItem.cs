using Newtonsoft.Json;

namespace patter_pal.dataservice.DataObjects
{
    public interface ContainerItem
    {
        [JsonProperty(PropertyName = "id")]
        string Id { get; set; }
        string UserId { get; set; }
    }
}
