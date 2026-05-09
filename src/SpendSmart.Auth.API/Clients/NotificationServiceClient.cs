using SpendSmart.Auth.API.DTOs;
using System.Net.Http.Json;

namespace SpendSmart.Auth.API.Clients
{
    public interface INotificationServiceClient
    {
        Task BroadcastAsync(SendBulkDto dto, string bearerToken);
    }

    public class NotificationServiceClient : INotificationServiceClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<NotificationServiceClient> _logger;

        public NotificationServiceClient(HttpClient http, ILogger<NotificationServiceClient> logger)
        {
            _http   = http;
            _logger = logger;
        }

        public async Task BroadcastAsync(SendBulkDto dto, string bearerToken)
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await _http.PostAsJsonAsync("/api/notifications/broadcast", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}
