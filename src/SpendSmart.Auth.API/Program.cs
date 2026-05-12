using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpendSmart.Auth.API.Clients;
using SpendSmart.Auth.API.Data;
using SpendSmart.Auth.API.Repositories;
using SpendSmart.Auth.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Dependency Injection ──────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategorySeedingService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Named HTTP client for seeding categories in the Category microservice on user registration
builder.Services.AddHttpClient("CategoryService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:CategoryService"]!);
});

// Typed HTTP clients for cross-service admin analytics and notifications
builder.Services.AddHttpClient<IExpenseServiceClient, ExpenseServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ExpenseService"]!);
});

builder.Services.AddHttpClient<IIncomeServiceClient, IncomeServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:IncomeService"]!);
});

builder.Services.AddHttpClient<INotificationServiceClient, NotificationServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:NotificationService"]!);
});

// Singleton: token blacklist must outlive individual requests
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

// ─── Authentication ───────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    // Reject blacklisted (logged-out) tokens on every protected request
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var blacklist = context.HttpContext.RequestServices
                .GetRequiredService<ITokenBlacklistService>();

            var rawHeader = context.HttpContext.Request.Headers.Authorization.ToString();
            if (rawHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = rawHeader["Bearer ".Length..].Trim();
                if (blacklist.IsRevoked(token))
                    context.Fail("Token has been revoked.");
            }

            return Task.CompletedTask;
        }
    };
})
// Google OAuth — redirect-based flow
// Credentials are stored in appsettings.json under "GoogleOAuth"
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, cookieOptions =>
{
    // Keep the OAuth state cookie alive long enough for the round-trip
    cookieOptions.Cookie.SameSite  = SameSiteMode.Lax;
    cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    cookieOptions.Cookie.IsEssential  = true;
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    var google = builder.Configuration.GetSection("GoogleOAuth");
    options.ClientId     = google["ClientId"]!;
    options.ClientSecret = google["ClientSecret"]!;
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.SaveTokens   = true;

    // Request the user's profile picture
    options.Scope.Add("profile");

    // Fix: Correlation cookie must survive the Google redirect (cross-site top-level navigation)
    options.CorrelationCookie.SameSite    = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.CorrelationCookie.IsEssential  = true;
    options.CorrelationCookie.HttpOnly     = true;
});

// ─── Authorization ────────────────────────────────────────────────────────────
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Allow essential cookies (like the OAuth correlation cookie) regardless of consent policy
builder.Services.Configure<CookiePolicyOptions>(o =>
{
    o.MinimumSameSitePolicy = SameSiteMode.Lax;
    o.CheckConsentNeeded    = _ => false;  // don't require consent banner
});

// ─── CORS ─────────────────────────────────────────────────────────────────────
var frontendUrl = builder.Configuration["GoogleOAuth:FrontendUrl"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ─── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SpendSmart Auth API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme      = "Bearer",
        BearerFormat = "JWT",
        In          = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Initialize database BEFORE any middleware runs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    try
    {
        db.Database.EnsureCreated();
        Console.WriteLine("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
        throw;
    }
}

// ─── Middleware Pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCookiePolicy();          // must be before UseAuthentication
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-create database tables
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

