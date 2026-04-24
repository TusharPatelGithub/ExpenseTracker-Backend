namespace SpendSmart.Report.API.DTOs
{
    public class ReportResponseDto
    {
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public string? FilePath { get; set; }
        public string Parameters { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
