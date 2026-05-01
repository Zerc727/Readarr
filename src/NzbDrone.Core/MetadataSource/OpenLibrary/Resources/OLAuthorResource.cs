using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NzbDrone.Core.MetadataSource.OpenLibrary.Resources
{
    public class OLAuthorResource
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("bio")]
        public OLTextValue Bio { get; set; }

        [JsonPropertyName("photos")]
        public List<int> Photos { get; set; } = new List<int>();

        [JsonPropertyName("links")]
        public List<OLLink> Links { get; set; } = new List<OLLink>();
    }

    public class OLAuthorWorksResponse
    {
        [JsonPropertyName("entries")]
        public List<OLWorkResource> Entries { get; set; } = new List<OLWorkResource>();

        [JsonPropertyName("size")]
        public int Size { get; set; }
    }

    public class OLTextValue
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        public static implicit operator string(OLTextValue v) => v?.Value;
    }

    public class OLLink
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}
