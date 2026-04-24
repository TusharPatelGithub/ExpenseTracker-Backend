using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart.Budget.API.Entities
{
    public class BudgetEntity
    {
        [Key]
        public int BudgetId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LimitAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SpentAmount { get; set; } = 0;

        [MaxLength(10)]
        public string Currency { get; set; } = "INR";

        [MaxLength(20)]
        public string Period { get; set; } = "MONTHLY";

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal GetRemainingAmount() => LimitAmount - SpentAmount;
    }
}
