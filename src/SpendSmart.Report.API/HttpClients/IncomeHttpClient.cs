namespace SpendSmart.Report.API.HttpClients
{
    public class IncomeHttpClient
    {
        private readonly HttpClient _httpClient;

        public IncomeHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetTotalByUserAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"/api/incomes/total");
            if (!response.IsSuccessStatusCode) return 0;
            var result = await response.Content.ReadFromJsonAsync<TotalResponse>();
            return result?.Total ?? 0;
        }

        public async Task<List<IncomeDto>> GetAllByUserAsync()
        {
            var response = await _httpClient.GetAsync($"/api/incomes/user");
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<IncomeDto>>() ?? new();
        }

        private class TotalResponse { public decimal Total { get; set; } }
    }

    public class IncomeDto
    {
        public int IncomeId { get; set; }
        public string Source { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
