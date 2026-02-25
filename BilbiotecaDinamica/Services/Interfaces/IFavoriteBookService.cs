using System.Collections.Generic;
using System.Threading.Tasks;
using BilbiotecaDinamica.Models;

namespace BilbiotecaDinamica.Services.Interfaces
{
    public interface IFavoriteBookService
    {
        Task<List<FavoriteBook>> GetByUserAsync(string userId);
        Task AddFavoriteAsync(FavoriteBook book);
        Task<FavoriteBook?> GetByIdAsync(int id, string userId);
        Task DeleteAsync(FavoriteBook book);
    }
}
