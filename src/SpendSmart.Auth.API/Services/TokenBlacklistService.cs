using System.Collections.Concurrent;

namespace SpendSmart.Auth.API.Services
{
    /// <summary>
    /// In-memory JWT token blacklist. Revoked tokens are stored until expiry.
    /// Registered as a Singleton so the set persists for the application lifetime.
    /// For multi-instance / production use, replace with a Redis-backed implementation.
    /// </summary>
    public class TokenBlacklistService : ITokenBlacklistService
    {
        // ConcurrentHashSet via ConcurrentDictionary — thread-safe
        private readonly ConcurrentDictionary<string, byte> _revokedTokens = new();

        public void RevokeToken(string token)
            => _revokedTokens.TryAdd(token, 0);

        public bool IsRevoked(string token)
            => _revokedTokens.ContainsKey(token);
    }
}
