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
        public List<int> Covers { get; set; } = new List<int>();

        [JsonPropertyName("subjects")]
        public List<string> Subjects { get; set; } = new List<string>();

        [JsonPropertyName("authors")]
        public List<OLWorkAuthorRef> Authors { get; set; } = new List<OLWorkAuthorRef>();

        [JsonPropertyName("links")]
        public List<OLLink> Links { get; set; } = new List<OLLink>();

        [JsonPropertyName("series")]
        public List<OLWorkSeriesRef> Series { get; set; } = new List<OLWorkSeriesRef>();
    }

    public class OLWorkSeriesRef
    {
        [JsonPropertyName("series")]
        public OLKeyRef SeriesKey { get; set; }

        [JsonPropertyName("position")]
        public string Position { get; set; }
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
        public List<OLEditionResource> Entries { get; set; } = new List<OLEditionResource>();

        [JsonPropertyName("size")]
        public int Size { get; set; }
    }
}
