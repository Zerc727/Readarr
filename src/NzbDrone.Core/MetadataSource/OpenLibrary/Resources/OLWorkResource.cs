using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NzbDrone.Core.MetadataSource.OpenLibrary.Resources
{
    public class OLWorkResource
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public OLTextValue Description { get; set; }

        [JsonPropertyName("covers")]
        public List<int> Covers { get; set; } = new();

        [JsonPropertyName("subjects")]
        public List<string> Subjects { get; set; } = new();

        [JsonPropertyName("authors")]
        public List<OLWorkAuthorRef> Authors { get; set; } = new();

        [JsonPropertyName("links")]
        public List<OLLink> Links { get; set; } = new();
    }

    public class OLWorkAuthorRef
    {
        [JsonPropertyName("author")]
        public OLKeyRef Author { get; set; }
    }

    public class OLKeyRef
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }
    }

    public class OLWorkEditionsResponse
    {
        [JsonPropertyName("entries")]
        public List<OLEditionResource> Entries { get; set; } = new();

        [JsonPropertyName("size")]
        public int Size { get; set; }
    }
}
