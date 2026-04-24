using System.ComponentModel.DataAnnotations;

namespace SpendSmart.Budget.API.DTOs
{
    public class UpdateBudgetDto
    {
        public int? CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal LimitAmount { get; set; }

        public string Currency { get; set; } = "INR";

        [Required]
        public string Period { get; set; } = "MONTHLY";

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
