namespace SpendSmart.Auth.API.Services
{
    /// <summary>
    /// Tracks revoked JWT tokens to support server-side logout.
    /// The in-memory implementation is suitable for development and single-instance
    /// deployments. Replace with a Redis-backed implementation for production.
    /// </summary>
    public interface ITokenBlacklistService
    {
        void RevokeToken(string token);
        bool IsRevoked(string token);
    }
}
