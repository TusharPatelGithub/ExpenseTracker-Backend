namespace SpendSmart.Budget.API.IntegrationEvents
{
    public class ExpenseCreatedEvent
    {
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
