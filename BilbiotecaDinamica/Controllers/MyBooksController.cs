using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BilbiotecaDinamica.Data;
using BilbiotecaDinamica.Models;
using System.Linq;
using System.Threading.Tasks;
using BilbiotecaDinamica.Services.Interfaces;

namespace BilbiotecaDinamica.Controllers
{
    [Authorize]
    public class MyBooksController : Controller
    {
        private readonly IFavoriteBookService _favoriteBookService;
        private readonly UserManager<IdentityUser> _userManager;

        public MyBooksController(IFavoriteBookService favoriteBookService, UserManager<IdentityUser> userManager)
        {
            _favoriteBookService = favoriteBookService;
            _userManager = userManager;
        }

        // GET: MyBooks
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var myBooks = await _favoriteBookService.GetByUserAsync(userId);
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

            await _favoriteBookService.AddFavoriteAsync(book);

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
            var favoriteBook = await _favoriteBookService.GetByIdAsync(id.Value, userId);
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
            var favoriteBook = await _favoriteBookService.GetByIdAsync(id, userId);
            if (favoriteBook != null)
            {
                await _favoriteBookService.DeleteAsync(favoriteBook);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
