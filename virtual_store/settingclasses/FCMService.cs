using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace virtual_store.Services
{
    public class FCMService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public FCMService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<bool> SendNotificationAsync(string fcmToken, string title, string body)
        {
            var serverKey = _configuration["FCM:ServerKey"]; // Get from appsettings.json

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("key", serverKey);

            var message = new
            {
                to = fcmToken,
                notification = new
                {
                    title = title,
                    body = body,
                    click_action = "https://yourfrontend.com", // optional
                    icon = "https://yourcdn.com/logo.png" // optional
                }
            };

            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://fcm.googleapis.com/fcm/send", content);
            return response.IsSuccessStatusCode;
        }
    }
}
