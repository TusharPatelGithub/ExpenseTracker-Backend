using System.ComponentModel.DataAnnotations;

namespace SpendSmart.Report.API.Entities
{
    public class ReportEntity
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReportType { get; set; } = "MONTHLY";

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public string? FilePath { get; set; }

        public string Parameters { get; set; } = "{}";

        [MaxLength(20)]
        public string Status { get; set; } = "GENERATED";
    }
}
