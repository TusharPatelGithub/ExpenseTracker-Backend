namespace SpendSmart.Auth.API.Services
{
    /// <summary>
    /// Calls the Category microservice (POST /api/categories/seed) via IHttpClientFactory
    /// to seed default expense/income categories for a newly registered user.
    /// Falls back with a warning log if the Category service is unreachable,
    /// so registration itself is never blocked.
    /// </summary>
    public class CategorySeedingService : ICategoryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CategorySeedingService> _logger;

        public CategorySeedingService(
            IHttpClientFactory httpClientFactory,
            ILogger<CategorySeedingService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task SeedDefaultCategoriesAsync(int userId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CategoryService");

                // POST /api/categories/seed — seeds defaults for the given userId
                // The Category service endpoint reads userId from the JWT claim,
                // but we call the internal seed endpoint directly with userId in path.
                var response = await client.PostAsync($"/api/categories/seed/{userId}", null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Default categories seeded successfully for UserId={UserId}.", userId);
                }
                else
                {
                    _logger.LogWarning(
                        "Category service returned {StatusCode} when seeding for UserId={UserId}.",
                        response.StatusCode, userId);
                }
            }
            catch (Exception ex)
            {
                // Log and swallow — registration must not fail because Category service is down
                _logger.LogError(ex,
                    "Failed to seed default categories for UserId={UserId}. " +
                    "Categories can be seeded later via POST /api/categories/seed.", userId);
            }
        }
    }
}
