using System;
using System.ComponentModel.DataAnnotations;

namespace BilbiotecaDinamica.Models
{
    public class SearchCacheEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SearchQuery { get; set; }

        public DateTime Timestamp { get; set; }

        [Required]
        public string SearchResultsJson { get; set; } // Stores serialized list of OpenLibrarySearchResult
    }
}
