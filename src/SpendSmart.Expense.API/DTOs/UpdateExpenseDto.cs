using System.ComponentModel.DataAnnotations;

namespace SpendSmart.Expense.API.DTOs
{
    public class UpdateExpenseDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "INR";

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [RegularExpression("^(CASH|CARD|UPI|NET_BANKING|WALLET)$",
            ErrorMessage = "PaymentMode must be CASH, CARD, UPI, NET_BANKING, or WALLET")]
        public string PaymentMode { get; set; } = "CASH";

        public string Tags { get; set; } = string.Empty;

        public bool IsRecurring { get; set; } = false;
    }
}