using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BilbiotecaDinamica.Data;
using BilbiotecaDinamica.Models;
using System.Linq;
using System.Threading.Tasks;

namespace BilbiotecaDinamica.Controllers
{
    [Authorize]
    public class MyBooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyBooksController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MyBooks
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var myBooks = await _context.FavoriteBooks
                                        .Where(b => b.UserId == userId)
                                        .ToListAsync();
            return View(myBooks);
        }

        // POST: MyBooks/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string openLibraryId, string title, string author, int? coverId, int? firstPublishYear, string? coverImageUrl)
        {
            if (openLibraryId == null || title == null || author == null)
            {
                return BadRequest("Missing required book information.");
            }

            var userId = _userManager.GetUserId(User);

            // Check if the book already exists for the user
            var existingBook = await _context.FavoriteBooks
                                            .FirstOrDefaultAsync(b => b.UserId == userId && b.OpenLibraryId == openLibraryId);

            if (existingBook == null)
            {
                var book = new FavoriteBook
                {
                    UserId = userId,
                    OpenLibraryId = openLibraryId,
                    Title = title,
                    Author = author,
                    CoverId = coverId,
                    FirstPublishYear = (int?)firstPublishYear,
                    CoverImageUrl = coverImageUrl
                };

                _context.Add(book);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home"); // Redirect back to the search results
        }

        // GET: MyBooks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var favoriteBook = await _context.FavoriteBooks
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (favoriteBook == null)
            {
                return NotFound();
            }

            return View(favoriteBook);
        }

        // POST: MyBooks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var favoriteBook = await _context.FavoriteBooks.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (favoriteBook != null)
            {
                _context.FavoriteBooks.Remove(favoriteBook);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
