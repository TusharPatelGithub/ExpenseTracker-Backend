using System.ComponentModel.DataAnnotations;

namespace SpendSmart.Income.API.DTOs
{
    public class UpdateIncomeDto
    {
        [Required]
        public string Source { get; set; } = "OTHER";

        [Required]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "INR";

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        public bool IsRecurring { get; set; } = false;

        public string? RecurrenceType { get; set; }
    }
}
