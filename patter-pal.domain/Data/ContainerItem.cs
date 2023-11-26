using Newtonsoft.Json;

namespace patter_pal.domain.Data
{
    public interface ContainerItem
    {
        [JsonProperty(PropertyName = "id")]
        string Id { get; set; }
        string UserId { get; set; }
    }
}
