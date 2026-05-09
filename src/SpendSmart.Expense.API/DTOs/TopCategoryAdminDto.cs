namespace SpendSmart.Expense.API.DTOs
{
    /// <summary>Used by admin analytics: top spending categories platform-wide.</summary>
    public class TopCategoryAdminDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = $"Category";
        public decimal TotalAmount { get; set; }
    }
}
