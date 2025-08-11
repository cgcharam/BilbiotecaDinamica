
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BilbiotecaDinamica.Models
{
    public class OpenLibrarySearchResult
    {
        [JsonPropertyName("docs")]
        public List<Doc>? Docs { get; set; }
    }

    public class Doc
    {
        [JsonPropertyName("title")]
        public required string Title { get; set; }

        [JsonPropertyName("author_name")]
        public List<string>? AuthorName { get; set; }

        [JsonPropertyName("first_publish_year")]
        public int FirstPublishYear { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object>? AdditionalFields { get; set; }
    }
}
