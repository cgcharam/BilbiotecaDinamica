using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BilbiotecaDinamica.Data;
using BilbiotecaDinamica.Models;
using BilbiotecaDinamica.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BilbiotecaDinamica.Services.Implementations
{
    public class SearchService : ISearchService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SearchService> _logger;
        private const int CacheTtlMinutes = 15;

        public SearchService(IHttpClientFactory httpClientFactory, ApplicationDbContext context, ILogger<SearchService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
        }

        public async Task<List<Doc>> SearchAsync(string query, string searchType)
        {
            if (string.IsNullOrEmpty(query)) return new List<Doc>();

            var searchField = searchType switch
            {
                "all" => "q",
                "text" => "q",
                "lists" => "",
                _ => searchType ?? "author",
            };

            if (string.IsNullOrEmpty(searchField)) return new List<Doc>();

            // Check cache
            var cachedEntry = await _context.SearchCacheEntries.FirstOrDefaultAsync(e => e.SearchQuery == query);

            if (cachedEntry != null && (DateTime.Now - cachedEntry.Timestamp).TotalMinutes < CacheTtlMinutes)
            {
                try
                {
                    return JsonSerializer.Deserialize<List<Doc>>(cachedEntry.SearchResultsJson) ?? new List<Doc>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize cached search results for query {Query}", query);
                }
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://openlibrary.org/search.json?{searchField}={Uri.EscapeDataString(query)}");
            if (!response.IsSuccessStatusCode) return new List<Doc>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OpenLibrarySearchResult>(json);
            var model = result?.Docs?.Take(15).ToList() ?? new List<Doc>();

            var searchResultsJson = JsonSerializer.Serialize(model);
            if (cachedEntry == null)
            {
                _context.SearchCacheEntries.Add(new SearchCacheEntry
                {
                    SearchQuery = query,
                    Timestamp = DateTime.Now,
                    SearchResultsJson = searchResultsJson
                });
            }
            else
            {
                cachedEntry.Timestamp = DateTime.Now;
                cachedEntry.SearchResultsJson = searchResultsJson;
                _context.SearchCacheEntries.Update(cachedEntry);
            }

            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<List<Doc>> AdvancedSearchAsync(AdvancedSearchViewModel model)
        {
            var queryParts = new List<string>();
            if (!string.IsNullOrEmpty(model.Title)) queryParts.Add($"title={model.Title}");
            if (!string.IsNullOrEmpty(model.Author)) queryParts.Add($"author={model.Author}");
            if (!string.IsNullOrEmpty(model.Subject)) queryParts.Add($"subject={model.Subject}");
            if (!string.IsNullOrEmpty(model.Query)) queryParts.Add($"q={model.Query}");

            if (!queryParts.Any()) return new List<Doc>();

            var fullQuery = string.Join("&", queryParts);

            // Check cache
            var cachedEntry = await _context.SearchCacheEntries.FirstOrDefaultAsync(e => e.SearchQuery == fullQuery);

            if (cachedEntry != null && (DateTime.Now - cachedEntry.Timestamp).TotalMinutes < CacheTtlMinutes)
            {
                try
                {
                    return JsonSerializer.Deserialize<List<Doc>>(cachedEntry.SearchResultsJson) ?? new List<Doc>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize cached advanced search results for query {Query}", fullQuery);
                }
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://openlibrary.org/search.json?{Uri.EscapeDataString(fullQuery)}");
            if (!response.IsSuccessStatusCode) return new List<Doc>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OpenLibrarySearchResult>(json);
            var list = result?.Docs?.Take(15).ToList() ?? new List<Doc>();

            var searchResultsJson = JsonSerializer.Serialize(list);
            if (cachedEntry == null)
            {
                _context.SearchCacheEntries.Add(new SearchCacheEntry
                {
                    SearchQuery = fullQuery,
                    Timestamp = DateTime.Now,
                    SearchResultsJson = searchResultsJson
                });
            }
            else
            {
                cachedEntry.Timestamp = DateTime.Now;
                cachedEntry.SearchResultsJson = searchResultsJson;
                _context.SearchCacheEntries.Update(cachedEntry);
            }

            await _context.SaveChangesAsync();
            return list;
        }
    }
}
