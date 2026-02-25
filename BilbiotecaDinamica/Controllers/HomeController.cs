using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BilbiotecaDinamica.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http; // Added this line
using Microsoft.AspNetCore.Localization; // Added this line
using System; // Added this line
using BilbiotecaDinamica.Services.Interfaces; // Use search service
using Microsoft.Extensions.Logging;

namespace BilbiotecaDinamica.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISearchService _searchService;

        public HomeController(ILogger<HomeController> logger, ISearchService searchService)
        {
            _logger = logger;
            _searchService = searchService;
        }

        public async Task<IActionResult> Index(string query, string searchType)
        {
            if (searchType == "advanced")
            {
                return RedirectToAction("AdvancedSearch");
            }

            var model = await _searchService.SearchAsync(query, searchType);

            ViewData["SearchType"] = searchType;
            return View(model);
        }

        public IActionResult AdvancedSearch()
        {
            return View(new AdvancedSearchViewModel());
        }

        public async Task<IActionResult> AdvancedSearchResults(AdvancedSearchViewModel searchModel)
        {
            var model = await _searchService.AdvancedSearchAsync(searchModel);
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

