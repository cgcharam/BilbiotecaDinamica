using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BilbiotecaDinamica.Models
{
    public class FavoriteBook
    {
        [Key]
        public int Id { get; set; }

        public required string UserId { get; set; }
        public IdentityUser? User { get; set; }

        public required string OpenLibraryId { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public int? CoverId { get; set; }
        public int? FirstPublishYear { get; set; }
    }
}
