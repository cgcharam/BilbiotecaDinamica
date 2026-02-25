using System.Collections.Generic;
using System.Threading.Tasks;
using BilbiotecaDinamica.Models;

namespace BilbiotecaDinamica.Services.Interfaces
{
    public interface ISearchService
    {
        Task<List<Doc>> SearchAsync(string query, string searchType);
        Task<List<Doc>> AdvancedSearchAsync(AdvancedSearchViewModel model);
    }
}
