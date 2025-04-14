using api.Models;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class OneSignalService
{
    private readonly HttpClient _httpClient;
    private readonly OneSignal _oneSignal;

    public OneSignalService(HttpClient httpClient, IOptions<OneSignal> oneSignal)
    {
        _httpClient = httpClient;
        _oneSignal = oneSignal.Value;
    }

    public async Task SendNotificationAll(string title, string message)
    {
        var request = new
        {
            app_id = _oneSignal.AppId,
            included_segments = new[] { "All" },
            headings = new { en = title },
            contents = new { en = message }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _oneSignal.Key);

        var response = await _httpClient.PostAsync("https://onesignal.com/api/v1/notifications", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendNotificationList(string title, string message, List<string> playerIds)
    {
        if (playerIds == null || playerIds.Count == 0) return;
        var request = new
        {
            app_id = _oneSignal.AppId,
            include_player_ids = playerIds,
            headings = new { en = title },
            contents = new { en = message }
        };

        var json = JsonSerializer.Serialize(request);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _oneSignal.Key);

        var response = await _httpClient.PostAsync("https://onesignal.com/api/v1/notifications", content);
        response.EnsureSuccessStatusCode();
    }
}
