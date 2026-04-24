using SpendSmart.Budget.API.DTOs;
using SpendSmart.Budget.API.Entities;
using SpendSmart.Budget.API.Repositories;

namespace SpendSmart.Budget.API.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly INotificationService _notificationService;

        public BudgetService(IBudgetRepository budgetRepository, INotificationService notificationService)
        {
            _budgetRepository = budgetRepository;
            _notificationService = notificationService;
        }

        public async Task<BudgetResponseDto> CreateBudgetAsync(int userId, CreateBudgetDto dto)
        {
            var budget = new BudgetEntity
            {
                UserId = userId,
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                LimitAmount = dto.LimitAmount,
                Currency = dto.Currency,
                Period = dto.Period,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true
            };

            await _budgetRepository.AddAsync(budget);
            await _budgetRepository.SaveChangesAsync();

            return MapToResponseDto(budget);
        }

        public async Task<BudgetResponseDto?> GetBudgetByIdAsync(int budgetId)
        {
            var budget = await _budgetRepository.FindByBudgetIdAsync(budgetId);
            return budget == null ? null : MapToResponseDto(budget);
        }

        public async Task<List<BudgetResponseDto>> GetBudgetsByUserAsync(int userId)
        {
            var budgets = await _budgetRepository.FindByUserIdAsync(userId);
            return budgets.Select(MapToResponseDto).ToList();
        }

        public async Task<List<BudgetResponseDto>> GetActiveBudgetsAsync(int userId)
        {
            var budgets = await _budgetRepository.FindActiveByUserIdAsync(userId);
            return budgets.Select(MapToResponseDto).ToList();
        }

        public async Task<BudgetResponseDto?> GetBudgetByCategoryAsync(int userId, int categoryId)
        {
            var budget = await _budgetRepository.FindByCategoryIdAsync(userId, categoryId);
            return budget == null ? null : MapToResponseDto(budget);
        }

        public async Task<List<BudgetResponseDto>> GetByPeriodAsync(int userId, string period)
        {
            var budgets = await _budgetRepository.FindByPeriodAsync(userId, period);
            return budgets.Select(MapToResponseDto).ToList();
        }

        public async Task<BudgetResponseDto> UpdateBudgetAsync(int budgetId, int userId, UpdateBudgetDto dto)
        {
            var budget = await _budgetRepository.FindByBudgetIdAsync(budgetId)
                ?? throw new KeyNotFoundException("Budget not found.");

            if (budget.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized to update this budget.");

            budget.CategoryId = dto.CategoryId;
            budget.Name = dto.Name;
            budget.LimitAmount = dto.LimitAmount;
            budget.Currency = dto.Currency;
            budget.Period = dto.Period;
            budget.StartDate = dto.StartDate;
            budget.EndDate = dto.EndDate;

            await _budgetRepository.UpdateAsync(budget);
            await _budgetRepository.SaveChangesAsync();

            return MapToResponseDto(budget);
        }

        public async Task DeleteBudgetAsync(int budgetId, int userId)
        {
            var budget = await _budgetRepository.FindByBudgetIdAsync(budgetId)
                ?? throw new KeyNotFoundException("Budget not found.");

            if (budget.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized to delete this budget.");

            await _budgetRepository.DeleteByBudgetIdAsync(budgetId);
        }

        public async Task UpdateSpentAmountAsync(int budgetId, decimal amount)
            => await _budgetRepository.UpdateSpentAmountAsync(budgetId, amount);

        public async Task<List<BudgetResponseDto>> GetOverBudgetAlertsAsync(int userId)
        {
            var budgets = await _budgetRepository.FindOverBudgetAsync(userId);
            return budgets.Select(MapToResponseDto).ToList();
        }

        public async Task<decimal> GetBudgetUtilizationAsync(int userId)
        {
            var budgets = await _budgetRepository.FindActiveByUserIdAsync(userId);
            if (!budgets.Any()) return 0;

            var totalLimit = budgets.Sum(b => b.LimitAmount);
            var totalSpent = budgets.Sum(b => b.SpentAmount);

            return totalLimit == 0 ? 0 : (totalSpent / totalLimit) * 100;
        }

        public async Task CheckBudgetOnExpenseAsync(int userId, int categoryId, decimal amount)
        {
            var budget = await _budgetRepository.FindByCategoryIdAsync(userId, categoryId);
            if (budget == null) return;

            await _budgetRepository.UpdateSpentAmountAsync(budget.BudgetId, amount);

            var updated = await _budgetRepository.FindByBudgetIdAsync(budget.BudgetId);
            if (updated == null) return;

            var utilization = updated.LimitAmount == 0 ? 0 : (updated.SpentAmount / updated.LimitAmount) * 100;

            if (updated.SpentAmount >= updated.LimitAmount)
                await _notificationService.SendBudgetAlertAsync(userId, updated.BudgetId, updated.Name, utilization, "LIMIT_REACHED");
            else if (utilization >= 80)
                await _notificationService.SendBudgetAlertAsync(userId, updated.BudgetId, updated.Name, utilization, "WARNING");
        }

        private static BudgetResponseDto MapToResponseDto(BudgetEntity b) => new()
        {
            BudgetId = b.BudgetId,
            UserId = b.UserId,
            CategoryId = b.CategoryId,
            Name = b.Name,
            LimitAmount = b.LimitAmount,
            SpentAmount = b.SpentAmount,
            RemainingAmount = b.GetRemainingAmount(),
            Currency = b.Currency,
            Period = b.Period,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt
        };
    }
}
