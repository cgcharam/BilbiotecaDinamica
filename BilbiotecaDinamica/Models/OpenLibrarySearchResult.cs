
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

        [JsonPropertyName("cover_i")] // This is the property for cover ID from Open Library
        public int? CoverId { get; set; }

        public string? CoverImageUrl
        {
            get
            {
                return CoverId.HasValue ? $"https://covers.openlibrary.org/b/id/{CoverId}-M.jpg" : null;
            }
        }

        [JsonExtensionData]
        public Dictionary<string, object>? AdditionalFields { get; set; }
    }
}
