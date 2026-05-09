using SpendSmart.Auth.API.DTOs;
using System.Net.Http.Json;

namespace SpendSmart.Auth.API.Clients
{
    public interface IExpenseServiceClient
    {
        /// <summary>Total expenses across ALL users on the platform.</summary>
        Task<decimal> GetPlatformTotalAsync(string bearerToken);

        /// <summary>Top N spending categories platform-wide.</summary>
        Task<List<TopCategoryDto>> GetTopCategoriesAsync(string bearerToken, int topN = 5);
    }

    public class ExpenseServiceClient : IExpenseServiceClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ExpenseServiceClient> _logger;

        public ExpenseServiceClient(HttpClient http, ILogger<ExpenseServiceClient> logger)
        {
            _http   = http;
            _logger = logger;
        }

        public async Task<decimal> GetPlatformTotalAsync(string bearerToken)
        {
            try
            {
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

                var result = await _http.GetFromJsonAsync<TotalWrapper>("/api/expenses/admin/total");
                return result?.Total ?? 0m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExpenseServiceClient: failed to fetch platform total.");
                return 0m;
            }
        }

        public async Task<List<TopCategoryDto>> GetTopCategoriesAsync(string bearerToken, int topN = 5)
        {
            try
            {
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

                var result = await _http.GetFromJsonAsync<List<TopCategoryDto>>(
                    $"/api/expenses/admin/top-categories?topN={topN}");
                return result ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExpenseServiceClient: failed to fetch top categories.");
                return new();
            }
        }

        private class TotalWrapper { public decimal Total { get; set; } }
    }
}
