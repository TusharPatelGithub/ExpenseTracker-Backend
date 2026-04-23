using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart.Expense.API.Entities
{
    public class ExpenseEntity
    {
        [Key]
        public int ExpenseId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [MaxLength(10)]
        public string Currency { get; set; } = "INR";
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [Required]
        public DateTime Date { get; set; }
        [MaxLength(20)]
        public string PaymentMode { get; set; } = "CASH";
        public string? ReceiptUrl { get; set; }
        [MaxLength(500)]
        public string Tags { get; set; } = string.Empty;
        public bool IsRecurring { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}