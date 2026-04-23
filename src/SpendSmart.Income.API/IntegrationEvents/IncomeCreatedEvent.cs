namespace SpendSmart.Income.API.IntegrationEvents
{
    public class IncomeCreatedEvent
    {
        public int UserId { get; set; }
        public string Source { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
