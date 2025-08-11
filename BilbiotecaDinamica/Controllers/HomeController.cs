using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BilbiotecaDinamica.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http; // Added this line
using Microsoft.AspNetCore.Localization; // Added this line
using System; // Added this line

namespace BilbiotecaDinamica.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string query, string searchType)
        {
            if (searchType == "advanced")
            {
                return RedirectToAction("AdvancedSearch");
            }

            var model = new List<Doc>();
            if (!string.IsNullOrEmpty(query))
            {
                var client = _httpClientFactory.CreateClient();
                var searchField = searchType switch
                {
                    "all" => "q",
                    "text" => "q",
                    "lists" => "", // Not supported
                    _ => searchType ?? "author",
                };

                if (!string.IsNullOrEmpty(searchField))
                {
                    var response = await client.GetAsync($"https://openlibrary.org/search.json?{searchField}={query}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<OpenLibrarySearchResult>(json);
                        model = result?.Docs ?? new List<Doc>();
                    }
                }
            }
            ViewData["SearchType"] = searchType;
            return View(model);
        }

        public IActionResult AdvancedSearch()
        {
            return View(new AdvancedSearchViewModel());
        }

        public async Task<IActionResult> AdvancedSearchResults(AdvancedSearchViewModel searchModel)
        { 
            var queryParts = new List<string>();
            if (!string.IsNullOrEmpty(searchModel.Title)) queryParts.Add($"title={searchModel.Title}");
            if (!string.IsNullOrEmpty(searchModel.Author)) queryParts.Add($"author={searchModel.Author}");
            if (!string.IsNullOrEmpty(searchModel.Subject)) queryParts.Add($"subject={searchModel.Subject}");
            if (!string.IsNullOrEmpty(searchModel.Query)) queryParts.Add($"q={searchModel.Query}");

            var model = new List<Doc>();
            if (queryParts.Any())
            {
                var client = _httpClientFactory.CreateClient();
                var queryString = string.Join("&", queryParts);
                var response = await client.GetAsync($"https://openlibrary.org/search.json?{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<OpenLibrarySearchResult>(json);
                    model = result?.Docs ?? new List<Doc>();
                }
            }

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
