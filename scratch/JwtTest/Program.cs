using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens; // .NET 8 default

class Program
{
    static void Main()
    {
        var secret = "SpendSmart@SuperSecretKey#2026!XyZ123456789";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "2"),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.Name, "Test"),
            new Claim(ClaimTypes.Role, "User")
        };
        
        var token = new JwtSecurityToken(
            issuer: "SpendSmart.Auth.API",
            audience: "SpendSmart.Client",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        var handler = new JsonWebTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "SpendSmart.Auth.API",
            ValidAudience = "SpendSmart.Client",
            IssuerSigningKey = key
        };
        
        try
        {
            var result = handler.ValidateToken(tokenString, validationParameters);
            if (result.IsValid)
            {
                var principal = new ClaimsPrincipal(result.ClaimsIdentity);
                var claimValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                Console.WriteLine("JsonWebTokenHandler NameIdentifier: " + (claimValue ?? "NULL"));
                
                foreach(var c in principal.Claims) {
                    Console.WriteLine("Claim: " + c.Type + " = " + c.Value);
                }
            }
            else {
                Console.WriteLine("Invalid token");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Validation error: " + ex.Message);
        }
    }
}
