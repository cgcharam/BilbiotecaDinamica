using System;
using System.ComponentModel.DataAnnotations;

namespace BilbiotecaDinamica.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nombre completo")]
        public string FullName { get; set; } = null!;

        // Owner of this author entry (each user has private authors list)
        [Required]
        public string UserId { get; set; } = null!;

        public Microsoft.AspNetCore.Identity.IdentityUser? User { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Ciudad de procedencia")]
        public string? City { get; set; }

        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string? Email { get; set; }
    }
}