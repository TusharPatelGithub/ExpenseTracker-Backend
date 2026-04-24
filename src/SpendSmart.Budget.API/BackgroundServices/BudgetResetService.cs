using Microsoft.EntityFrameworkCore;
using SpendSmart.Budget.API.Data;

namespace SpendSmart.Budget.API.BackgroundServices
{
    public class BudgetResetService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BudgetResetService> _logger;

        public BudgetResetService(IServiceScopeFactory scopeFactory, ILogger<BudgetResetService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRun = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                var delay = nextRun - now;

                _logger.LogInformation("BudgetResetService: Next reset scheduled at {NextRun}", nextRun);

                await Task.Delay(delay, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();

                await db.Budgets
                    .Where(b => b.Period == "MONTHLY" && b.IsActive)
                    .ExecuteUpdateAsync(b => b.SetProperty(p => p.SpentAmount, 0), stoppingToken);

                _logger.LogInformation("BudgetResetService: Monthly budgets reset successfully.");
            }
        }
    }
}
