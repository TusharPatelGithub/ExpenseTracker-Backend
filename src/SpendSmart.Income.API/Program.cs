using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpendSmart.Income.API.BackgroundServices;
using SpendSmart.Income.API.Data;
using SpendSmart.Income.API.Repositories;
using SpendSmart.Income.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IncomeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("ExpenseService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ExpenseService"]!);
});

builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddHttpContextAccessor(); // needed for JWT forwarding in GetNetBalanceAsync

// Named HTTP client for cross-service calls to the Notification microservice
builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:NotificationService"]!);
});

// IHostedService: sends RECURRING_REMINDER notifications for upcoming recurring incomes
builder.Services.AddHostedService<RecurringReminderService>();

// MassTransit (InMemory — no RabbitMQ needed on Render)
builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory();
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
    c.SwaggerDoc("v1", new() { Title = "SpendSmart Income API", Version = "v1" });
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



// Create tables with raw SQL — works even when EnsureCreated skips due to shared DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IncomeDbContext>();
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""Incomes"" (
                ""IncomeId"" SERIAL PRIMARY KEY,
                ""UserId"" INTEGER NOT NULL,
                ""Source"" CHARACTER VARYING(50) NOT NULL,
                ""Amount"" NUMERIC(18,2) NOT NULL,
                ""Currency"" CHARACTER VARYING(10),
                ""Description"" CHARACTER VARYING(500),
                ""Date"" TIMESTAMP WITH TIME ZONE NOT NULL,
                ""IsRecurring"" BOOLEAN NOT NULL DEFAULT FALSE,
                ""RecurrenceType"" CHARACTER VARYING(20),
                ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                ""UpdatedAt"" TIMESTAMP WITH TIME ZONE
            )");
        Console.WriteLine("Incomes table OK.");
    }
    catch (Exception ex) { Console.WriteLine($"Incomes table: {ex.Message}"); }
    
    try { await db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Incomes_UserId"" ON ""Incomes"" (""UserId"")"); } catch { }
    try { await db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Incomes_Source"" ON ""Incomes"" (""Source"")"); } catch { }
    try { await db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Incomes_Date"" ON ""Incomes"" (""Date"")"); } catch { }
}

app.Run();
