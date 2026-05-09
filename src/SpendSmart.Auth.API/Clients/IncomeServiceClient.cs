using System.Net.Http.Json;

namespace SpendSmart.Auth.API.Clients
{
    public interface IIncomeServiceClient
    {
        /// <summary>Total income across ALL users on the platform.</summary>
        Task<decimal> GetPlatformTotalAsync(string bearerToken);
    }

    public class IncomeServiceClient : IIncomeServiceClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<IncomeServiceClient> _logger;

        public IncomeServiceClient(HttpClient http, ILogger<IncomeServiceClient> logger)
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

                var result = await _http.GetFromJsonAsync<TotalWrapper>("/api/incomes/admin/total");
                return result?.Total ?? 0m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IncomeServiceClient: failed to fetch platform total.");
                return 0m;
            }
        }

        private class TotalWrapper { public decimal Total { get; set; } }
    }
}
