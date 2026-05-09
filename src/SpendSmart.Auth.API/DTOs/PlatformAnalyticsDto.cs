namespace SpendSmart.Auth.API.DTOs
{
    /// <summary>
    /// Aggregate platform-wide stats returned by GET /api/admin/analytics.
    /// Totals for expenses and income are fetched via HTTP from the respective microservices.
    /// </summary>
    public class PlatformAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalIncome { get; set; }
        public List<TopCategoryDto> TopSpendingCategories { get; set; } = new();
    }

    public class TopCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
