using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);
        
        // 1. Login
        var loginPayload = """{"email":"test2031@test.com","password":"Test@123"}""";
        var loginContent = new StringContent(loginPayload, Encoding.UTF8, "application/json");
        var loginRes = await client.PostAsync("https://expensetracker-auth.onrender.com/api/users/login", loginContent);
        var loginJson = await loginRes.Content.ReadAsStringAsync();
        Console.WriteLine("Login: " + (int)loginRes.StatusCode);
        Console.WriteLine(loginJson);
        
        var tokenStart = loginJson.IndexOf("\"token\":\"") + 9;
        if (tokenStart < 9) { Console.WriteLine("No token found"); return; }
        var tokenEnd = loginJson.IndexOf("\"", tokenStart);
        var token = loginJson.Substring(tokenStart, tokenEnd - tokenStart);
        Console.WriteLine("\nToken extracted OK");
        
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        // 2. POST income
        Console.WriteLine("\n--- POST /api/incomes ---");
        var incomePayload = """{"source":"SALARY","amount":100000,"currency":"USD","description":"Monthly Salary","date":"2026-05-13T00:00:00Z","isRecurring":false}""";
        var incomeContent = new StringContent(incomePayload, Encoding.UTF8, "application/json");
        var incomeRes = await client.PostAsync("https://expensetracker-income.onrender.com/api/incomes", incomeContent);
        Console.WriteLine("Status: " + (int)incomeRes.StatusCode);
        Console.WriteLine(await incomeRes.Content.ReadAsStringAsync());
        
        // 3. GET notifications/unread-count
        Console.WriteLine("\n--- GET /api/notifications/unread-count ---");
        var notRes = await client.GetAsync("https://expensetracker-notification.onrender.com/api/notifications/unread-count");
        Console.WriteLine("Status: " + (int)notRes.StatusCode);
        Console.WriteLine(await notRes.Content.ReadAsStringAsync());
    }
}
