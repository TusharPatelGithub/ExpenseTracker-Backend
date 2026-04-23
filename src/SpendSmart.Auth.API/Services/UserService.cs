using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SpendSmart.Auth.API.DTOs;
using SpendSmart.Auth.API.Entities;
using SpendSmart.Auth.API.Repositories;

namespace SpendSmart.Auth.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICategoryService _categoryService;
        private readonly ITokenBlacklistService _tokenBlacklist;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(
            IUserRepository userRepository,
            ICategoryService categoryService,
            ITokenBlacklistService tokenBlacklist,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _categoryService = categoryService;
            _tokenBlacklist = tokenBlacklist;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
        }

        // ─── Register ────────────────────────────────────────────────────────────

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _userRepository.ExistsByEmailAsync(dto.Email))
                throw new Exception("Email already registered.");

            var user = new User
            {
                FullName = dto.FullName,
                Email    = dto.Email,
                Currency = dto.Currency
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Seed default categories (Food, Transport, etc.) for the new user
            await _categoryService.SeedDefaultCategoriesAsync(user.UserId);

            return new AuthResponseDto
            {
                Token    = GenerateJwtToken(user),
                FullName = user.FullName,
                Email    = user.Email,
                Currency = user.Currency,
                Role     = user.Role
            };
        }

        // ─── Login ───────────────────────────────────────────────────────────────

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.FindByEmailAsync(dto.Email)
                ?? throw new Exception("Invalid email or password.");

            if (!user.IsActive)
                throw new Exception("Account is suspended. Please contact support.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new Exception("Invalid email or password.");

            await _userRepository.UpdateLastLoginAsync(user.UserId, DateTime.UtcNow);

            return new AuthResponseDto
            {
                Token    = GenerateJwtToken(user),
                FullName = user.FullName,
                Email    = user.Email,
                Currency = user.Currency,
                Role     = user.Role
            };
        }

        // ─── Logout ──────────────────────────────────────────────────────────────

        public Task LogoutAsync(string token)
        {
            _tokenBlacklist.RevokeToken(token);
            return Task.CompletedTask;
        }

        // ─── Google OAuth ────────────────────────────────────────────────────────

        /// <summary>
        /// Called from the Google OAuth callback. Finds an existing user by email,
        /// or creates a new one (no password hash — Google-only account).
        /// Default currency is INR; user can update it after first login.
        /// </summary>
        public async Task<AuthResponseDto> HandleGoogleLoginAsync(
            string email, string fullName, string avatarUrl)
        {
            var user = await _userRepository.FindByEmailAsync(email);
            bool isNew = user is null;

            if (isNew)
            {
                user = new User
                {
                    FullName      = fullName,
                    Email         = email,
                    AvatarUrl     = avatarUrl,
                    PasswordHash  = string.Empty, // Google-authenticated, no local password
                    Currency      = "INR"
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                // Seed default categories for brand-new Google users
                await _categoryService.SeedDefaultCategoriesAsync(user.UserId);
            }
            else
            {
                if (!user!.IsActive)
                    throw new Exception("Account is suspended. Please contact support.");

                await _userRepository.UpdateLastLoginAsync(user.UserId, DateTime.UtcNow);
            }

            return new AuthResponseDto
            {
                Token    = GenerateJwtToken(user!),
                FullName = user!.FullName,
                Email    = user.Email,
                Currency = user.Currency,
                Role     = user.Role
            };
        }

        // ─── Profile ─────────────────────────────────────────────────────────────

        public async Task<User?> GetUserByIdAsync(int userId)
            => await _userRepository.FindByUserIdAsync(userId);

        public async Task UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.FindByUserIdAsync(userId)
                ?? throw new Exception("User not found.");

            user.FullName  = dto.FullName;
            user.AvatarUrl = dto.AvatarUrl;

            await _userRepository.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.FindByUserIdAsync(userId)
                ?? throw new Exception("User not found.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
            if (result == PasswordVerificationResult.Failed)
                throw new Exception("Current password is incorrect.");

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            await _userRepository.SaveChangesAsync();
        }

        public async Task UpdateCurrencyAsync(int userId, string currency)
            => await _userRepository.UpdateCurrencyAsync(userId, currency);

        // ─── Account Management ───────────────────────────────────────────────────

        /// <summary>Self-service — logged-in user deactivates their own account.</summary>
        public async Task DeactivateAccountAsync(int userId)
        {
            var user = await _userRepository.FindByUserIdAsync(userId)
                ?? throw new Exception("User not found.");

            user.IsActive = false;
            await _userRepository.SaveChangesAsync();
        }

        /// <summary>Admin — suspend any user (IsActive = false).</summary>
        public async Task SuspendAccountAsync(int targetUserId)
        {
            var user = await _userRepository.FindByUserIdAsync(targetUserId)
                ?? throw new Exception($"User {targetUserId} not found.");

            user.IsActive = false;
            await _userRepository.SaveChangesAsync();
        }

        /// <summary>Admin — permanently delete a user row from the database.</summary>
        public async Task DeleteAccountAsync(int targetUserId)
        {
            var exists = await _userRepository.FindByUserIdAsync(targetUserId);
            if (exists is null)
                throw new Exception($"User {targetUserId} not found.");

            await _userRepository.DeleteByIdAsync(targetUserId);
        }

        /// <summary>Admin — all users including suspended accounts.</summary>
        public async Task<List<User>> GetAllUsersAsync()
            => await _userRepository.FindAllAsync();

        // ─── JWT Helpers ──────────────────────────────────────────────────────────

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email,           user.Email),
                new Claim(ClaimTypes.Name,            user.FullName),
                new Claim(ClaimTypes.Role,            user.Role)
            };

            var token = new JwtSecurityToken(
                issuer:             jwtSettings["Issuer"],
                audience:           jwtSettings["Audience"],
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(24),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}