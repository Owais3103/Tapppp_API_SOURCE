using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

public class FirebaseNotificationSender
{
    private readonly string _firebaseProjectId;
    private readonly string _serviceAccountPath;

    public FirebaseNotificationSender(string firebaseProjectId, string serviceAccountJsonPath)
    {
        _firebaseProjectId = firebaseProjectId;
        _serviceAccountPath = serviceAccountJsonPath;
    }

    public async Task SendNotificationAsync(string deviceToken, string title, string body)
    {
        var message = new
        {
            message = new
            {
                token = deviceToken,
                notification = new
                {
                    title = title,
                    body = body
                }
            }
        };

        var payload = JsonConvert.SerializeObject(message);
        var accessToken = await GetAccessTokenAsync();

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://fcm.googleapis.com/v1/projects/{_firebaseProjectId}/messages:send")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string> GetAccessTokenAsync()
    {
        GoogleCredential credential = GoogleCredential
            .FromFile(_serviceAccountPath)
            .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

        var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        return accessToken;
    }
}
