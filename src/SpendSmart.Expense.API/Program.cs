using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpendSmart.Expense.API.Data;
using SpendSmart.Expense.API.Repositories;
using SpendSmart.Expense.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Database (PostgreSQL) ────────────────────────────────────────────────────
builder.Services.AddDbContext<ExpenseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Dependency Injection ─────────────────────────────────────────────────────
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IMediaService, LocalMediaService>();

// ── MassTransit (InMemory — no RabbitMQ needed on Render) ─────────────────────
builder.Services.AddMassTransit(x =>{    x.UsingInMemory();});

// ── HTTP Clients (Budget service for direct spend updates) ───────────────────
builder.Services.AddHttpClient("BudgetService", client =>
{
    var url = builder.Configuration["ServiceUrls:BudgetService"] 
              ?? "https://expensetracker-budget.onrender.com";
    client.BaseAddress = new Uri(url);
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpContextAccessor();


// ── JWT Authentication ───────────────────────────────────────────────────────
var jwt = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// ── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SpendSmart Expense API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {{
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }});
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



// Auto-create database tables if they don't exist
// Create tables with raw SQL — works even when EnsureCreated skips due to shared DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""Expenses"" (
                ""ExpenseId"" SERIAL PRIMARY KEY,
                ""UserId"" INTEGER NOT NULL,
                ""CategoryId"" INTEGER NOT NULL,
                ""Amount"" NUMERIC(18,2) NOT NULL,
                ""Currency"" CHARACTER VARYING(10),
                ""Description"" CHARACTER VARYING(500),
                ""Date"" TIMESTAMP WITH TIME ZONE NOT NULL,
                ""PaymentMode"" CHARACTER VARYING(20),
                ""ReceiptUrl"" TEXT,
                ""Tags"" CHARACTER VARYING(500),
                ""IsRecurring"" BOOLEAN NOT NULL DEFAULT FALSE,
                ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                ""UpdatedAt"" TIMESTAMP WITH TIME ZONE
            )");
        Console.WriteLine("Expenses table OK.");
    }
    catch (Exception ex) { Console.WriteLine($"Expenses table: {ex.Message}"); }
    
    try { await db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Expenses_UserId"" ON ""Expenses"" (""UserId"")"); } catch { }
    try { await db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Expenses_CategoryId"" ON ""Expenses"" (""CategoryId"")"); } catch { }
    try { await db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Expenses_Date"" ON ""Expenses"" (""Date"")"); } catch { }
}

app.Run();
