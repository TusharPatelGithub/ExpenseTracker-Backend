using MassTransit;
using SpendSmart.Expense.API.IntegrationEvents;
using SpendSmart.Budget.API.Services;

namespace SpendSmart.Budget.API.Consumers
{
    public class BudgetCheckConsumer : IConsumer<ExpenseCreatedEvent>
    {
        private readonly IBudgetService _budgetService;

        public BudgetCheckConsumer(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        public async Task Consume(ConsumeContext<ExpenseCreatedEvent> context)
        {
            var msg = context.Message;
            await _budgetService.CheckBudgetOnExpenseAsync(msg.UserId, msg.CategoryId, msg.Amount);
        }
    }
}
