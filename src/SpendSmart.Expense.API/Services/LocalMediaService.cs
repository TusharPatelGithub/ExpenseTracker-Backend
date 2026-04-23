namespace SpendSmart.Expense.API.Services
{
    public class LocalMediaService : IMediaService
    {
        private readonly IWebHostEnvironment _env;

        public LocalMediaService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadReceiptAsync(IFormFile file, int userId)
        {
            var folder = Path.Combine(_env.WebRootPath ?? "wwwroot", "receipts", userId.ToString());
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return relative URL
            return $"/receipts/{userId}/{fileName}";
        }
    }
}