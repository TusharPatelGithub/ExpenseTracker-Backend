namespace SpendSmart.Report.API.HttpClients
{
    public class ExpenseHttpClient
    {
        private readonly HttpClient _httpClient;

        public ExpenseHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetTotalByUserAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"/api/expenses/total");
            if (!response.IsSuccessStatusCode) return 0;
            var result = await response.Content.ReadFromJsonAsync<TotalResponse>();
            return result?.Total ?? 0;
        }

        public async Task<List<CategoryAmountDto>> GetCategoryBreakdownAsync(int userId, DateTime start, DateTime end)
        {
            var response = await _httpClient.GetAsync($"/api/expenses/user");
            if (!response.IsSuccessStatusCode) return new();
            var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseDto>>();
            if (expenses == null) return new();

            return expenses
                .Where(e => e.Date >= start && e.Date <= end)
                .GroupBy(e => e.CategoryId)
                .Select(g => new CategoryAmountDto
                {
                    CategoryId = g.Key,
                    Total = g.Sum(e => e.Amount)
                }).ToList();
        }

        public async Task<List<ExpenseDto>> GetAllByUserAsync()
        {
            var response = await _httpClient.GetAsync($"/api/expenses/user");
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<ExpenseDto>>() ?? new();
        }

        private class TotalResponse { public decimal Total { get; set; } }
    }

    public class ExpenseDto
    {
        public int ExpenseId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
    }

    public class CategoryAmountDto
    {
        public int CategoryId { get; set; }
        public decimal Total { get; set; }
    }
}
