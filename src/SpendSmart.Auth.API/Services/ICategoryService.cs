namespace SpendSmart.Auth.API.Services
{
    /// <summary>
    /// Called after a new user registers to seed default expense categories
    /// (Food, Transport, Shopping, etc.) on their account.
    /// In a full microservices deployment this would call the Category microservice
    /// or publish a UserRegistered integration event.
    /// </summary>
    public interface ICategoryService
    {
        Task SeedDefaultCategoriesAsync(int userId);
    }
}
