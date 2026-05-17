using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

var client = new HttpClient();
client.Timeout = TimeSpan.FromSeconds(30);

// 1. Login with the test user
var loginPayload = """{"email":"test2031@test.com","password":"Test@123"}""";
var loginRes = await client.PostAsync(
    "https://expensetracker-auth.onrender.com/api/users/login",
    new StringContent(loginPayload, Encoding.UTF8, "application/json"));
var loginJson = await loginRes.Content.ReadAsStringAsync();
Console.WriteLine("Login: " + (int)loginRes.StatusCode);

var tokenStart = loginJson.IndexOf("\"token\":\"") + 9;
var tokenEnd = loginJson.IndexOf("\"", tokenStart);
var token = loginJson.Substring(tokenStart, tokenEnd - tokenStart);
Console.WriteLine("Token: OK");

client.DefaultRequestHeaders.Authorization = 
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

// 2. POST /api/incomes
Console.WriteLine("\n--- POST /api/incomes ---");
var incomePayload = """{"source":"SALARY","amount":100000,"currency":"USD","description":"Monthly Salary","date":"2026-05-13T00:00:00Z","isRecurring":false}""";
var incomeRes = await client.PostAsync(
    "https://expensetracker-income.onrender.com/api/incomes",
    new StringContent(incomePayload, Encoding.UTF8, "application/json"));
Console.WriteLine("Status: " + (int)incomeRes.StatusCode);
Console.WriteLine(await incomeRes.Content.ReadAsStringAsync());

// 3. GET /api/incomes (verify it saved)
Console.WriteLine("\n--- GET /api/incomes ---");
var listRes = await client.GetAsync("https://expensetracker-income.onrender.com/api/incomes");
Console.WriteLine("Status: " + (int)listRes.StatusCode);
Console.WriteLine(await listRes.Content.ReadAsStringAsync());

// 4. Notifications
Console.WriteLine("\n--- GET /api/notifications/unread-count ---");
var notRes = await client.GetAsync("https://expensetracker-notification.onrender.com/api/notifications/unread-count");
Console.WriteLine("Status: " + (int)notRes.StatusCode);
Console.WriteLine(await notRes.Content.ReadAsStringAsync());
