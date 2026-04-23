namespace SpendSmart.Auth.API.Services
{
    /// <summary>
    /// Stub implementation of ICategoryService for the Auth microservice.
    /// In the full microservices setup, this will be replaced by an HTTP client
    /// that calls the Category microservice (POST /api/categories/seed/{userId}),
    /// or by publishing a UserRegistered integration event via a message bus.
    /// </summary>
    public class CategorySeedingService : ICategoryService
    {
        private readonly ILogger<CategorySeedingService> _logger;

        // Default categories seeded for every new user
        private static readonly string[] DefaultCategories =
        [
            "Food & Dining",
            "Transport",
            "Shopping",
            "Entertainment",
            "Health & Medical",
            "Utilities",
            "Education",
            "Travel",
            "Personal Care",
            "Others"
        ];

        public CategorySeedingService(ILogger<CategorySeedingService> logger)
        {
            _logger = logger;
        }

        public Task SeedDefaultCategoriesAsync(int userId)
        {
            // TODO: Replace with HTTP call to Category microservice when available.
            // e.g., await _httpClient.PostAsync($"api/categories/seed/{userId}", null);
            _logger.LogInformation(
                "SeedDefaultCategories called for UserId={UserId}. " +
                "Categories to seed: [{Categories}]",
                userId,
                string.Join(", ", DefaultCategories));

            return Task.CompletedTask;
        }
    }
}
