using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpendSmart.Category.API.Data;
using SpendSmart.Category.API.Repositories;
using SpendSmart.Category.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CategoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

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
    c.SwaggerDoc("v1", new() { Title = "SpendSmart Category API", Version = "v1" });
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
    var db = scope.ServiceProvider.GetRequiredService<CategoryDbContext>();
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""Categories"" (
                ""CategoryId"" SERIAL PRIMARY KEY,
                ""UserId"" INTEGER,
                ""Name"" CHARACTER VARYING(100) NOT NULL,
                ""Icon"" CHARACTER VARYING(10),
                ""Color"" CHARACTER VARYING(20),
                ""Type"" CHARACTER VARYING(20) NOT NULL,
                ""IsDefault"" BOOLEAN NOT NULL DEFAULT FALSE,
                ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL
            )");
        Console.WriteLine("Categories table OK.");
    }
    catch (Exception ex) { Console.WriteLine($"Categories table: {ex.Message}"); }
    
    try { await db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Categories_UserId"" ON ""Categories"" (""UserId"")"); } catch { }

    // Seed default categories if none exist yet
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""Categories"" (""UserId"", ""Name"", ""Icon"", ""Color"", ""Type"", ""IsDefault"", ""IsActive"", ""CreatedAt"")
            SELECT NULL, v.name, v.icon, v.color, v.type, TRUE, TRUE, NOW()
            FROM (VALUES
                ('Food & Dining',   '🍔', '#FF6B6B', 'EXPENSE'),
                ('Transport',       '🚗', '#4ECDC4', 'EXPENSE'),
                ('Shopping',        '🛍️', '#45B7D1', 'EXPENSE'),
                ('Healthcare',      '💊', '#96CEB4', 'EXPENSE'),
                ('Entertainment',   '🎬', '#FFEAA7', 'EXPENSE'),
                ('Utilities',       '💡', '#DDA0DD', 'EXPENSE'),
                ('Education',       '📚', '#AED6F1', 'EXPENSE'),
                ('Housing',         '🏠', '#F8C471', 'EXPENSE'),
                ('Salary',          '💰', '#98D8C8', 'INCOME'),
                ('Freelance',       '💻', '#F7DC6F', 'INCOME'),
                ('Investment',      '📈', '#82E0AA', 'INCOME'),
                ('Rental',          '🏘️', '#F1948A', 'INCOME')
            ) AS v(name, icon, color, type)
            WHERE NOT EXISTS (
                SELECT 1 FROM ""Categories"" WHERE ""IsDefault"" = TRUE AND ""Name"" = v.name
            )");
        Console.WriteLine("Default categories seeded OK.");
    }
    catch (Exception ex) { Console.WriteLine($"Category seeding: {ex.Message}"); }
}

app.Run();
