using System.ComponentModel.DataAnnotations;

namespace SpendSmart.Category.API.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Icon { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Color { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "EXPENSE";
    }
}
