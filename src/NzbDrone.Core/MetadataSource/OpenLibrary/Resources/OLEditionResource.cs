using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NzbDrone.Core.MetadataSource.OpenLibrary.Resources
{
    public class OLEditionResource
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("isbn_13")]
        public List<string> Isbn13 { get; set; } = new();

        [JsonPropertyName("isbn_10")]
        public List<string> Isbn10 { get; set; } = new();

        [JsonPropertyName("publishers")]
        public List<string> Publishers { get; set; } = new();

        [JsonPropertyName("publish_date")]
        public string PublishDate { get; set; }

        [JsonPropertyName("number_of_pages")]
        public int? NumberOfPages { get; set; }

        [JsonPropertyName("covers")]
        public List<int> Covers { get; set; } = new();

        [JsonPropertyName("languages")]
        public List<OLKeyRef> Languages { get; set; } = new();

        [JsonPropertyName("description")]
        public OLTextValue Description { get; set; }

        [JsonPropertyName("physical_format")]
        public string PhysicalFormat { get; set; }

        [JsonPropertyName("works")]
        public List<OLKeyRef> Works { get; set; } = new();

        [JsonPropertyName("identifiers")]
        public OLEditionIdentifiers Identifiers { get; set; }
    }

    public class OLEditionIdentifiers
    {
        [JsonPropertyName("amazon")]
        public List<string> Amazon { get; set; } = new();
    }
}
