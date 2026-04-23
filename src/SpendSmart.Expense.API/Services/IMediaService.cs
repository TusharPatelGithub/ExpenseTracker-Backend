namespace SpendSmart.Expense.API.Services
{
    public interface IMediaService
    {
        Task<string> UploadReceiptAsync(IFormFile file, int userId);
    }
}