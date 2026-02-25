using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BilbiotecaDinamica.Data;
using BilbiotecaDinamica.Models;
using BilbiotecaDinamica.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BilbiotecaDinamica.Services.Implementations
{
    public class FavoriteBookService : IFavoriteBookService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FavoriteBookService> _logger;

        public FavoriteBookService(ApplicationDbContext context, ILogger<FavoriteBookService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<FavoriteBook>> GetByUserAsync(string userId)
        {
            return await _context.FavoriteBooks.Where(b => b.UserId == userId).ToListAsync();
        }

        public async Task AddFavoriteAsync(FavoriteBook book)
        {
            // Limitar a máximo 10 favoritos por usuario
            var favCount = await _context.FavoriteBooks.CountAsync(b => b.UserId == book.UserId);
            if (favCount >= 10)
            {
                throw new InvalidOperationException("El número máximo de libros favoritos (10) ha sido alcanzado.");
            }

            var exists = await _context.FavoriteBooks.AnyAsync(b => b.UserId == book.UserId && b.OpenLibraryId == book.OpenLibraryId);
            if (!exists)
            {
                _context.FavoriteBooks.Add(book);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<FavoriteBook?> GetByIdAsync(int id, string userId)
        {
            return await _context.FavoriteBooks.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        }

        public async Task DeleteAsync(FavoriteBook book)
        {
            _context.FavoriteBooks.Remove(book);
            await _context.SaveChangesAsync();
        }
    }
}
