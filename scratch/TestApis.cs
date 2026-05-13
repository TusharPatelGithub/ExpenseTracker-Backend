using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new HttpClient();
        
        // 1. Register a new user
        var payload = "{\"email\":\"test2028@test.com\",\"password\":\"Test@123\",\"fullName\":\"Test User\",\"currency\":\"USD\"}";
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync("https://expensetracker-auth.onrender.com/api/users/register", content);
        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Auth Response:");
        Console.WriteLine(json);
        
        // Extract token manually
        var tokenStart = json.IndexOf("\"token\":\"") + 9;
        if (tokenStart > 8)
        {
            var tokenEnd = json.IndexOf("\"", tokenStart);
            var token = json.Substring(tokenStart, tokenEnd - tokenStart);
            
            Console.WriteLine("\nTesting Income API...");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var incRes = await client.PostAsync("https://expensetracker-income.onrender.com/api/incomes", new StringContent("{\"source\":\"Test\",\"amount\":100,\"currency\":\"USD\",\"description\":\"Test\",\"date\":\"2026-05-13T00:00:00Z\",\"isRecurring\":false}", Encoding.UTF8, "application/json"));
            Console.WriteLine((int)incRes.StatusCode);
            Console.WriteLine(await incRes.Content.ReadAsStringAsync());
            
            Console.WriteLine("\nTesting Notification API...");
            var notRes = await client.GetAsync("https://expensetracker-notification.onrender.com/api/notifications/unread-count");
            Console.WriteLine((int)notRes.StatusCode);
            Console.WriteLine(await notRes.Content.ReadAsStringAsync());
        }
    }
}
