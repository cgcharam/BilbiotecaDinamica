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
        private readonly ApplicationDbContext _db;
        private readonly BilbiotecaDinamica.Services.Interfaces.ISearchService _searchService;

        public MyBooksController(IFavoriteBookService favoriteBookService, UserManager<IdentityUser> userManager, ApplicationDbContext db, BilbiotecaDinamica.Services.Interfaces.ISearchService searchService)
        {
            _favoriteBookService = favoriteBookService;
            _userManager = userManager;
            _db = db;
            _searchService = searchService;
        }

        // GET: MyBooks
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var myBooks = await _favoriteBookService.GetByUserAsync(userId);
            // Obtener lista de autores del usuario para el formulario de añadido manual
            var authors = await _db.Authors.AsNoTracking().Where(a => a.UserId == userId).ToListAsync();

            var vm = new Models.MyBooksViewModel
            {
                FavoriteBooks = myBooks,
                Authors = authors
            };

            return View(vm);
        }

        // POST: MyBooks/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string openLibraryId, string title, string author, string? author_key, int? coverId, int? firstPublishYear, string? coverImageUrl, bool isManual = false, string? returnUrl = null)
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

            // Si es adición manual desde la sección [Mis libros], exigir que el autor exista
            if (isManual)
            {
                var existsAuthor = await _db.Authors.AnyAsync(a => a.FullName == author);
                if (!existsAuthor)
                {
                    TempData["Error"] = "El autor especificado no existe. Agrega el autor en 'Mis Autores' antes de añadir el libro.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                // Si viene desde Home (no manual), separar autores por comas y crear cada registro si no existe para el usuario
                var authorNames = author.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrEmpty(a)).ToList();
                var authorKeys = (author_key ?? string.Empty).Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k)).ToList();

                for (int i = 0; i < authorNames.Count; i++)
                {
                    var name = authorNames[i];
                    var key = i < authorKeys.Count ? authorKeys[i] : null;

                    var existsAuthor = await _db.Authors.FirstOrDefaultAsync(a => a.FullName == name && a.UserId == userId);
                    if (existsAuthor == null)
                    {
                        Author? fetched = null;

                        // Primero intentar por clave de OpenLibrary si se proporcionó
                        if (!string.IsNullOrEmpty(key))
                        {
                            fetched = await _searchService.SearchAuthorByKeyAsync(key);
                        }

                        // Si no se obtuvo por key, intentar búsqueda por nombre
                        if (fetched == null)
                        {
                            fetched = await _searchService.SearchAuthorAsync(name);
                        }

                        if (fetched != null)
                        {
                            fetched.UserId = userId;
                            if (string.IsNullOrEmpty(fetched.FullName)) fetched.FullName = name;
                            _db.Authors.Add(fetched);
                        }
                        else
                        {
                            _db.Authors.Add(new Author { FullName = name, UserId = userId });
                        }
                    }
                }

                await _db.SaveChangesAsync();
            }

            bool success = false;
            string message = string.Empty;
            try
            {
                await _favoriteBookService.AddFavoriteAsync(book);
                success = true;
                message = "Libro añadido correctamente.";
                TempData["Success"] = message;
            }
            catch (InvalidOperationException ex)
            {
                success = false;
                message = ex.Message;
                TempData["Error"] = message;
            }

            // If the request is AJAX, return JSON and do not redirect (client will handle UI)
            var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest")
                         || Request.Headers["Accept"].ToString().Contains("application/json");
            if (isAjax)
            {
                return Json(new { success, message });
            }

            // If request originated from Home (non-AJAX), redirect back to returnUrl or Home
            if (!isManual)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index");
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
