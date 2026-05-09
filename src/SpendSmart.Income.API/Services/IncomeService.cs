using MassTransit;
using Microsoft.Extensions.Logging;
using SpendSmart.Income.API.DTOs;
using SpendSmart.Income.API.Entities;
using SpendSmart.Income.API.IntegrationEvents;
using SpendSmart.Income.API.Repositories;

namespace SpendSmart.Income.API.Services
{
    public class IncomeService : IIncomeService
    {
        private readonly IIncomeRepository _incomeRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<IncomeService> _logger;

        public IncomeService(
            IIncomeRepository incomeRepository,
            IPublishEndpoint publishEndpoint,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<IncomeService> logger)
        {
            _incomeRepository     = incomeRepository;
            _publishEndpoint      = publishEndpoint;
            _httpClientFactory    = httpClientFactory;
            _httpContextAccessor  = httpContextAccessor;
            _logger               = logger;
        }

        public async Task<IncomeResponseDto> AddIncomeAsync(int userId, AddIncomeDto dto)
        {
            var income = new IncomeEntity
            {
                UserId = userId,
                Source = dto.Source,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Description = dto.Description,
                Date = dto.Date.ToUniversalTime(),
                IsRecurring = dto.IsRecurring,
                RecurrenceType = dto.RecurrenceType
            };

            await _incomeRepository.AddAsync(income);
            await _incomeRepository.SaveChangesAsync();

            await _publishEndpoint.Publish(new IncomeCreatedEvent
            {
                UserId = income.UserId,
                Source = income.Source,
                Amount = income.Amount,
                Timestamp = income.Date
            });

            return MapToResponseDto(income);
        }

        public async Task<IncomeResponseDto?> GetIncomeByIdAsync(int incomeId)
        {
            var income = await _incomeRepository.FindByIncomeIdAsync(incomeId);
            return income == null ? null : MapToResponseDto(income);
        }

        public async Task<List<IncomeResponseDto>> GetIncomesByUserAsync(int userId)
        {
            var incomes = await _incomeRepository.FindByUserIdAsync(userId);
            return incomes.Select(MapToResponseDto).ToList();
        }

        public async Task<List<IncomeResponseDto>> GetBySourceAsync(int userId, string source)
        {
            var incomes = await _incomeRepository.FindBySourceAsync(userId, source);
            return incomes.Select(MapToResponseDto).ToList();
        }

        public async Task<List<IncomeResponseDto>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var incomes = await _incomeRepository.FindByDateRangeAsync(userId, startDate, endDate);
            return incomes.Select(MapToResponseDto).ToList();
        }

        public async Task<IncomeResponseDto> UpdateIncomeAsync(int incomeId, int userId, UpdateIncomeDto dto)
        {
            var income = await _incomeRepository.FindByIncomeIdAsync(incomeId)
                ?? throw new KeyNotFoundException("Income not found.");

            if (income.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized to update this income.");

            income.Source = dto.Source;
            income.Amount = dto.Amount;
            income.Currency = dto.Currency;
            income.Description = dto.Description;
            income.Date = dto.Date.ToUniversalTime();
            income.IsRecurring = dto.IsRecurring;
            income.RecurrenceType = dto.RecurrenceType;

            await _incomeRepository.UpdateAsync(income);
            await _incomeRepository.SaveChangesAsync();

            return MapToResponseDto(income);
        }

        public async Task DeleteIncomeAsync(int incomeId, int userId)
        {
            var income = await _incomeRepository.FindByIncomeIdAsync(incomeId)
                ?? throw new KeyNotFoundException("Income not found.");

            if (income.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized to delete this income.");

            await _incomeRepository.DeleteByIncomeIdAsync(incomeId);

            _logger.LogInformation("AUDIT LOG: User {UserId} deleted Income {IncomeId} from {Source} of Amount {Amount} {Currency} on {Date}", 
                userId, incomeId, income.Source, income.Amount, income.Currency, DateTime.UtcNow);
        }

        public async Task<decimal> GetTotalIncomeAsync(int userId)
            => await _incomeRepository.SumByUserIdAsync(userId);

        public async Task<decimal> GetTotalBySourceAsync(int userId, string source)
            => await _incomeRepository.SumBySourceAsync(userId, source);

        public async Task<List<IncomeResponseDto>> GetRecurringIncomesAsync(int userId)
        {
            var incomes = await _incomeRepository.FindRecurringAsync(userId);
            return incomes.Select(MapToResponseDto).ToList();
        }

        public async Task<decimal> GetNetBalanceAsync(int userId)
        {
            var totalIncome = await _incomeRepository.SumByUserIdAsync(userId);

            var client = _httpClientFactory.CreateClient("ExpenseService");

            // Forward the caller's JWT so the Expense API can authorise the request
            var jwt = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(jwt))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", jwt.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase));

            // Correct endpoint: /api/expenses/total (userId resolved from JWT claim)
            var response = await client.GetAsync("/api/expenses/total");
            decimal totalExpense = 0;
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TotalResponse>();
                totalExpense = result?.Total ?? 0;
            }
            return totalIncome - totalExpense;
        }

        public async Task<decimal> GetPlatformTotalAsync()
            => await _incomeRepository.SumAllPlatformAsync();

        private static IncomeResponseDto MapToResponseDto(IncomeEntity e) => new()
        {
            IncomeId = e.IncomeId,
            UserId = e.UserId,
            Source = e.Source,
            Amount = e.Amount,
            Currency = e.Currency,
            Description = e.Description,
            Date = e.Date,
            IsRecurring = e.IsRecurring,
            RecurrenceType = e.RecurrenceType,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };

        private class TotalResponse
        {
            public decimal Total { get; set; }
        }
    }
}
