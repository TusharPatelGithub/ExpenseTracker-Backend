using MassTransit;
using Microsoft.AspNetCore.Http;
using SpendSmart.Expense.API.DTOs;
using SpendSmart.Expense.API.Entities;
using SpendSmart.Expense.API.IntegrationEvents;
using SpendSmart.Expense.API.Repositories;

namespace SpendSmart.Expense.API.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IMediaService _mediaService;
        private readonly IPublishEndpoint _publishEndpoint;

        public ExpenseService(
            IExpenseRepository expenseRepository,
            IMediaService mediaService,
            IPublishEndpoint publishEndpoint)
        {
            _expenseRepository = expenseRepository;
            _mediaService = mediaService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ExpenseResponseDto> AddExpenseAsync(int userId, AddExpenseDto dto, IFormFile? receipt)
        {
            string? receiptUrl = null;
            if (receipt != null)
            {
                receiptUrl = await _mediaService.UploadReceiptAsync(receipt, userId);
            }

            var expense = new ExpenseEntity
            {
                UserId = userId,
                CategoryId = dto.CategoryId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Description = dto.Description,
                Date = dto.Date,
                PaymentMode = dto.PaymentMode,
                Tags = dto.Tags,
                IsRecurring = dto.IsRecurring,
                ReceiptUrl = receiptUrl
            };

            await _expenseRepository.AddAsync(expense);
            await _expenseRepository.SaveChangesAsync();

            // Publish Integration Event to notify Budget Service
            await _publishEndpoint.Publish(new ExpenseCreatedEvent
            {
                UserId = expense.UserId,
                CategoryId = expense.CategoryId,
                Amount = expense.Amount,
                Timestamp = expense.Date
            });

            return MapToResponseDto(expense);
        }

        public async Task<ExpenseResponseDto?> GetExpenseByIdAsync(int expenseId)
        {
            var expense = await _expenseRepository.FindByExpenseIdAsync(expenseId);
            return expense == null ? null : MapToResponseDto(expense);
        }

        public async Task<List<ExpenseResponseDto>> GetExpensesByUserAsync(int userId)
        {
            var expenses = await _expenseRepository.FindByUserIdAsync(userId);
            return expenses.Select(MapToResponseDto).ToList();
        }

        public async Task<List<ExpenseResponseDto>> GetByCategoryAsync(int userId, int categoryId)
        {
            var expenses = await _expenseRepository.FindByUserIdAndCategoryAsync(userId, categoryId);
            return expenses.Select(MapToResponseDto).ToList();
        }

        public async Task<List<ExpenseResponseDto>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var expenses = await _expenseRepository.FindByDateRangeAsync(userId, startDate, endDate);
            return expenses.Select(MapToResponseDto).ToList();
        }

        public async Task<List<ExpenseResponseDto>> GetByPaymentModeAsync(int userId, string paymentMode)
        {
            var expenses = await _expenseRepository.FindByPaymentModeAsync(userId, paymentMode);
            return expenses.Select(MapToResponseDto).ToList();
        }

        public async Task<ExpenseResponseDto> UpdateExpenseAsync(int expenseId, int userId, UpdateExpenseDto dto)
        {
            var expense = await _expenseRepository.FindByExpenseIdAsync(expenseId)
                ?? throw new Exception("Expense not found.");

            if (expense.UserId != userId)
                throw new Exception("Unauthorized to update this expense.");

            expense.CategoryId = dto.CategoryId;
            expense.Amount = dto.Amount;
            expense.Currency = dto.Currency;
            expense.Description = dto.Description;
            expense.Date = dto.Date;
            expense.PaymentMode = dto.PaymentMode;
            expense.Tags = dto.Tags;
            expense.IsRecurring = dto.IsRecurring;

            await _expenseRepository.UpdateAsync(expense);
            await _expenseRepository.SaveChangesAsync();

            return MapToResponseDto(expense);
        }

        public async Task DeleteExpenseAsync(int expenseId, int userId)
        {
            var expense = await _expenseRepository.FindByExpenseIdAsync(expenseId)
                ?? throw new Exception("Expense not found.");

            if (expense.UserId != userId)
                throw new Exception("Unauthorized to delete this expense.");

            // Requirements specified hard delete
            await _expenseRepository.DeleteByExpenseIdAsync(expenseId);
        }

        public async Task<decimal> GetTotalByUserAsync(int userId)
            => await _expenseRepository.SumByUserIdAsync(userId);

        public async Task<decimal> GetTotalByCategoryAsync(int userId, int categoryId)
            => await _expenseRepository.SumByCategoryAsync(userId, categoryId);

        public async Task<List<ExpenseResponseDto>> GetRecurringExpensesAsync(int userId)
        {
            var expenses = await _expenseRepository.FindRecurringAsync(userId);
            return expenses.Select(MapToResponseDto).ToList();
        }

        public async Task<List<ExpenseResponseDto>> SearchExpensesAsync(int userId, string keyword)
        {
            var expenses = await _expenseRepository.SearchExpensesAsync(userId, keyword);
            return expenses.Select(MapToResponseDto).ToList();
        }

        private static ExpenseResponseDto MapToResponseDto(ExpenseEntity e) => new()
        {
            ExpenseId = e.ExpenseId,
            UserId = e.UserId,
            CategoryId = e.CategoryId,
            Amount = e.Amount,
            Currency = e.Currency,
            Description = e.Description,
            Date = e.Date,
            PaymentMode = e.PaymentMode,
            ReceiptUrl = e.ReceiptUrl,
            Tags = e.Tags,
            IsRecurring = e.IsRecurring,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}
