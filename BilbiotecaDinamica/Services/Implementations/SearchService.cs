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

        public async Task<Author?> SearchAuthorByKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            // Normalize key to start with /authors/
            var normalized = key.StartsWith("/authors/") ? key : (key.StartsWith("OL") ? $"/authors/{key}" : key);

            try
            {
                var client = _httpClientFactory.CreateClient();
                var detailsResp = await client.GetAsync($"https://openlibrary.org{normalized}.json");
                if (!detailsResp.IsSuccessStatusCode)
                {
                    return null;
                }

                var detailsJson = await detailsResp.Content.ReadAsStringAsync();
                using var detailsDoc = JsonDocument.Parse(detailsJson);
                var root = detailsDoc.RootElement;

                var authorName = root.TryGetProperty("name", out var n) ? n.GetString() : null;
                var birthDateStr = root.TryGetProperty("birth_date", out var bd) ? bd.GetString() : null;
                var birthPlace = root.TryGetProperty("birth_place", out var bp) ? bp.GetString() : null;
                if (string.IsNullOrEmpty(birthPlace))
                {
                    if (root.TryGetProperty("birthplace", out var bpx)) birthPlace = bpx.GetString();
                    else if (root.TryGetProperty("location", out var loc)) birthPlace = loc.GetString();
                }

                DateTime? dob = null;
                if (!string.IsNullOrEmpty(birthDateStr) && DateTime.TryParse(birthDateStr, out var parsed)) dob = parsed;

                return new Author
                {
                    FullName = authorName ?? key,
                    DateOfBirth = dob,
                    City = birthPlace
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch author details by key {Key}", key);
                return new Author { FullName = key };
            }
        }

        public async Task<Author?> SearchAuthorAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            try
            {
                var client = _httpClientFactory.CreateClient();
                // Search authors
                var response = await client.GetAsync($"https://openlibrary.org/search/authors.json?q={Uri.EscapeDataString(name)}");
                if (!response.IsSuccessStatusCode) return new Author { FullName = name };

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("docs", out var docs) || docs.GetArrayLength() == 0)
                {
                    return new Author { FullName = name };
                }

                var first = docs[0];
                var authorName = first.TryGetProperty("name", out var n) ? n.GetString() ?? name : name;
                var key = first.TryGetProperty("key", out var k) ? k.GetString() : null; // e.g. "/authors/OL123A"

                // If we have a key, fetch detailed record
                string birthDateStr = null;
                string birthPlace = null;
                if (!string.IsNullOrEmpty(key))
                {
                    var detailsResp = await client.GetAsync($"https://openlibrary.org/authors/{key}.json");
                    if (detailsResp.IsSuccessStatusCode)
                    {
                        var detailsJson = await detailsResp.Content.ReadAsStringAsync();
                        using var detailsDoc = JsonDocument.Parse(detailsJson);
                        var root = detailsDoc.RootElement;
                        if (root.TryGetProperty("birth_date", out var bd)) birthDateStr = bd.GetString();
                        if (root.TryGetProperty("birth_place", out var bp)) birthPlace = bp.GetString();
                        // sometimes 'location' or 'birthplace' fields may vary; try other names
                        if (string.IsNullOrEmpty(birthPlace))
                        {
                            if (root.TryGetProperty("birthplace", out var bpx)) birthPlace = bpx.GetString();
                            else if (root.TryGetProperty("location", out var loc)) birthPlace = loc.GetString();
                        }
                    }
                }

                DateTime? dob = null;
                if (!string.IsNullOrEmpty(birthDateStr) && DateTime.TryParse(birthDateStr, out var parsed)) dob = parsed;

                return new Author
                {
                    FullName = authorName,
                    DateOfBirth = dob,
                    City = birthPlace
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch author details for {Name}", name);
                return new Author { FullName = name };
            }
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
