namespace SpendSmart.Report.API.DTOs
{
    public class MonthlySummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetSavings { get; set; }
        public decimal SavingsRate { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
