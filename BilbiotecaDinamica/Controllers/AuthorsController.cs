using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BilbiotecaDinamica.Data;
using BilbiotecaDinamica.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BilbiotecaDinamica.Controllers
{
    [Authorize]
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;

        public AuthorsController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var authors = await _context.Authors.AsNoTracking().Where(a => a.UserId == userId).ToListAsync();
            return View(authors);
        }

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Author author)
        {
            if (!ModelState.IsValid)
                return View(author);

            var userId = _userManager.GetUserId(User);
            author.UserId = userId;
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = _userManager.GetUserId(User);
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (author == null) return NotFound();
            return View(author);
        }

        // POST: Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            if (id != author.Id) return NotFound();
            var userId = _userManager.GetUserId(User);
            if (author.UserId != userId) return Forbid();
            if (!ModelState.IsValid) return View(author);
            try
            {
                _context.Authors.Update(author);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Authors.AnyAsync(a => a.Id == id && a.UserId == userId)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var userId = _userManager.GetUserId(User);
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (author == null) return NotFound();
            return View(author);
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (author == null) return NotFound();
            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
