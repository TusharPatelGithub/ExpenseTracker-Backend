using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpendSmart.Budget.API.BackgroundServices;
using SpendSmart.Budget.API.Consumers;
using SpendSmart.Budget.API.Data;
using SpendSmart.Budget.API.Repositories;
using SpendSmart.Budget.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BudgetDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<BudgetResetService>();

// HTTP client for cross-service budget alert calls to the Notification microservice
builder.Services.AddHttpClient("NotificationService", client =>
{
    var url = builder.Configuration["ServiceUrls:NotificationService"]
              ?? "https://expensetracker-notification.onrender.com";
    client.BaseAddress = new Uri(url);
    client.Timeout = TimeSpan.FromSeconds(90); // Render free tier needs ~50s cold start
});

// MassTransit (InMemory — no RabbitMQ needed on Render)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BudgetCheckConsumer>();
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SpendSmart Budget API", Version = "v1" });
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



// Auto-create database tables if they don't exist
// Create tables with raw SQL — works even when EnsureCreated skips due to shared DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""Budgets"" (
                ""BudgetId"" SERIAL PRIMARY KEY,
                ""UserId"" INTEGER NOT NULL,
                ""CategoryId"" INTEGER,
                ""Name"" CHARACTER VARYING(100) NOT NULL,
                ""LimitAmount"" NUMERIC(18,2) NOT NULL,
                ""SpentAmount"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                ""Currency"" CHARACTER VARYING(10),
                ""Period"" CHARACTER VARYING(20),
                ""StartDate"" TIMESTAMP WITH TIME ZONE NOT NULL,
                ""EndDate"" TIMESTAMP WITH TIME ZONE NOT NULL,
                ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL
            )");
        Console.WriteLine("Budgets table OK.");
    }
    catch (Exception ex) { Console.WriteLine($"Budgets table: {ex.Message}"); }
    
    try { await db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Budgets_UserId"" ON ""Budgets"" (""UserId"")"); } catch { }
}

app.Run();
