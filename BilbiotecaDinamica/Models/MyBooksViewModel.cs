using System.Collections.Generic;

namespace BilbiotecaDinamica.Models
{
    public class MyBooksViewModel
    {
        public List<FavoriteBook> FavoriteBooks { get; set; } = new List<FavoriteBook>();
        public List<Author> Authors { get; set; } = new List<Author>();
    }
}