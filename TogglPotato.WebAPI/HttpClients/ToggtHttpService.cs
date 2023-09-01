using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.HttpClients;

public class TogglHttpService : ITogglHttpService
{
    private readonly HttpClient _httpClient;

    public TogglHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.track.toggl.com");
    }

    public async ValueTask<UserProfile> GetUserProfile(string apiKey, ILogger logger)
    {
        using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v9/me"))
        {
            requestMessage.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            string basicToken = Convert.ToBase64String(
                ASCIIEncoding.ASCII.GetBytes(
                    $"{apiKey}:api_token"
                )
            );
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicToken);

            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                logger.LogWarning(
                    "Toggl user profile request returned StatusCode {StatusCode}.",
                    response.StatusCode.ToString()
                );
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            return await response.Content.ReadFromJsonAsync<UserProfile>()
                ?? new UserProfile();
        }
    }

    public async ValueTask<List<TimeEntry>> GetDailyTimeEntries(
        DateTime startTime,
        DateTime endTime,
        string apiKey,
        ILogger logger
    )
    {
        Dictionary<string, string?> queryString = new Dictionary<string, string?>()
        {
            { "start_date", startTime.ToString("yyyy-MM-dd'T'HH:mm:ss.ff'Z'") },
            { "end_date", endTime.ToString("yyyy-MM-dd'T'HH:mm:ss.ff'Z'") }
        };

        string uri = QueryHelpers.AddQueryString("/api/v9/me/time_entries", queryString);
        using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
        {
            requestMessage.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            string basicToken = Convert.ToBase64String(
                ASCIIEncoding.ASCII.GetBytes(
                    $"{apiKey}:api_token"
                )
            );
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicToken);

            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                logger.LogWarning(
                    "Toggl daily time entries request returned StatusCode {StatusCode}.",
                    response.StatusCode.ToString()
                );
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            return await response.Content.ReadFromJsonAsync<List<TimeEntry>>()
                ?? new List<TimeEntry>();
        }
    }

    public async ValueTask<TimeEntry> UpdateTimeEntry(
        TimeEntry timeEntry,
        string apiKey,
        ILogger logger
    )
    {
        string uri = $"/api/v9/workspaces/{timeEntry.WorkspaceId}/time_entries/{timeEntry.Id}";
        using (HttpRequestMessage requestMessage = new HttpRequestMessage(
            HttpMethod.Put,
            uri
        ))
        {
            // TODO: this is not ideal, should make a copy of timeEntry object.
            timeEntry.Id = default(long);

            timeEntry.Start = new DateTime(timeEntry.Start.Ticks, DateTimeKind.Utc);
            timeEntry.Stop = new DateTime(timeEntry.Stop.Ticks, DateTimeKind.Utc);

            requestMessage.Content = JsonContent.Create<TimeEntry>(timeEntry);

            string basicToken = Convert.ToBase64String(
                ASCIIEncoding.ASCII.GetBytes(
                    $"{apiKey}:api_token"
                )
            );
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicToken);

            HttpResponseMessage response = _httpClient.Send(requestMessage);

            if (response.IsSuccessStatusCode == false)
            {
                logger.LogWarning("HTTP response error code: {ErrorCode}", response.StatusCode);
                logger.LogWarning(
                    "HTTP response error message: {ErrorMessage}", await response.Content.ReadAsStringAsync()
                );
            }

            TimeEntry teResponse = await response.Content.ReadFromJsonAsync<TimeEntry>()
                ?? throw new NotImplementedException();
            return teResponse;
        }
    }
}