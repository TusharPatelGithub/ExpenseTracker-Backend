using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart.Income.API.Entities
{
    public class IncomeEntity
    {
        [Key]
        public int IncomeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Source { get; set; } = "OTHER";

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "INR";

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        public bool IsRecurring { get; set; } = false;

        [MaxLength(20)]
        public string? RecurrenceType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
