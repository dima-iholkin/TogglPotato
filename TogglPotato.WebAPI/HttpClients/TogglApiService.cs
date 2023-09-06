using System.Net;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using OneOf;
using TogglPotato.WebAPI.Domain.Services;
using TogglPotato.WebAPI.HttpClients.Helpers;
using TogglPotato.WebAPI.HttpClients.ErrorHandling;
using TogglPotato.WebAPI.HttpClients.ErrorHandling.Models;
using TogglPotato.WebAPI.HttpClients.Models;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.HttpClients;

public class TogglApiService : ITogglApiService
{
    private readonly HttpClient _httpClient;
    private readonly GlobalTimeService _timeService;
    private readonly ILogger<TogglApiService> _logger;

    public TogglApiService(HttpClient httpClient, GlobalTimeService timeService, ILogger<TogglApiService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.track.toggl.com");
        _timeService = timeService;
        _logger = logger;
    }

    public async ValueTask<OneOf<UserProfile, TogglApiErrorResult>> GetUserProfileAsync(
        TogglApiKey togglApiKey, CancellationToken cancellationToken
    )
    {
        // 1. Configure and send the Toggl API request.

        HttpResponseMessage? response;

        using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v9/me"))
        {
            requestMessage.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            requestMessage.Headers.AddApiKeyAuthorization(togglApiKey);
            response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        }

        // 2. Deal with the API error responses.

        if (response.StatusCode != HttpStatusCode.OK)
        {
            TogglApiErrorResult errorResult = await TogglApiErrorHandler.HandleHttpErrorsAsync(response, _logger);
            return errorResult;
        }

        // 3. Deserialize the UserProfileResponse.

        UserProfileResponse? userProfileResponse = await response.Content.ReadFromJsonAsync<UserProfileResponse>(
            cancellationToken
        );

        // TODO: Produce a proper return for deserialization error.
        if (userProfileResponse == null)
        {
            throw new Exception("UserProfileResponse wasn't deserialized correctly.");
        }

        // 4. Return the UserProfile on success.

        UserProfile userProfile = userProfileResponse.ConvertToUserProfile(_timeService);
        return userProfile;
    }

    public async ValueTask<OneOf<List<TimeEntry>, TogglApiErrorResult>> GetDailyTimeEntriesAsync(
        TimeZoneInfo timezoneInfo, DateOnly date, TogglApiKey togglApiKey, CancellationToken cancellationToken
    )
    {
        // 1. Generate the start and end times for the day.

        (DateTime startTime, DateTime endTime) = _timeService.GenerateUtcTimeRangeForDailyTimeEntries(
            timezoneInfo, date
        );

        // 2. Configure and send a request to Toggl API.

        HttpResponseMessage? response;

        Dictionary<string, string?> queryString = new Dictionary<string, string?>()
        {
            { "start_date", startTime.ToTogglApiString() },
            { "end_date", endTime.ToTogglApiString() }
        };

        string uri = QueryHelpers.AddQueryString("/api/v9/me/time_entries", queryString);
        using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
        {
            requestMessage.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            requestMessage.Headers.AddApiKeyAuthorization(togglApiKey);
            response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        }

        // 3. Deal with the API error responses.

        if (response.StatusCode != HttpStatusCode.OK)
        {
            TogglApiErrorResult errorResult = await TogglApiErrorHandler.HandleHttpErrorsAsync(response, _logger);
            return errorResult;
        }

        // 4. Deserialize the List<TimeEntry>.

        List<TimeEntry>? timeEntries = await response.Content.ReadFromJsonAsync<List<TimeEntry>>(cancellationToken);

        if (timeEntries == null)
        {
            throw new Exception("List<TimeEntry> wasn't deserialized correctly.");
        }

        timeEntries.ForEach(te =>
        {
            te.Start = te.Start.ToUniversalTime();
        });

        return timeEntries;
    }

    private async ValueTask<OneOf<TimeEntry, TogglApiErrorResult>> UpdateTimeEntryAsync(
        TimeEntry timeEntry, TogglApiKey togglApiKey
    )
    {
        // 1. Configure and send a request to Toggl API.

        HttpResponseMessage? response;

        string uri = $"/api/v9/workspaces/{timeEntry.WorkspaceId}/time_entries/{timeEntry.Id}";
        using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, uri))
        {
            // 1.1 Format the TimeEntry to be sent.
            TimeEntry newTimeEntry = (TimeEntry)timeEntry.Clone();
            newTimeEntry.Id = default(long);
            newTimeEntry.Start = new DateTime(timeEntry.Start.Ticks, DateTimeKind.Utc);
            newTimeEntry.Stop = new DateTime(timeEntry.Stop.Ticks, DateTimeKind.Utc);
            newTimeEntry.CreatedWith = "TogglPotato";

            // 1.2 Configure the request.
            requestMessage.Content = JsonContent.Create<TimeEntry>(newTimeEntry);
            requestMessage.Headers.AddApiKeyAuthorization(togglApiKey);
            response = _httpClient.Send(requestMessage);
        }

        // 2. Deal with the API error responses.

        if (response.IsSuccessStatusCode == false)
        {
            TogglApiErrorResult errorResult = await TogglApiErrorHandler.HandleHttpErrorsAsync(response, _logger);
            return errorResult;
        }

        // 3. Deserialize the Time Entry.

        TimeEntry? updatedTimeEntry = await response.Content.ReadFromJsonAsync<TimeEntry>();

        if (updatedTimeEntry == null)
        {
            throw new Exception("TimeEntry wasn't deserialized correctly.");
        }

        // 4. Return the updated Time Entry.

        return updatedTimeEntry;
    }

    public async Task<OneOf<List<TimeEntry>, TogglApiErrorResult>> UpdateTimeEntriesAsync(
        List<TimeEntry> timeEntries, TogglApiKey togglApiKey, CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        List<TimeEntry> responseTimeEntries = new List<TimeEntry>();

        foreach (TimeEntry te in timeEntries)
        {
            if (te.Modified == true)
            {
                OneOf<TimeEntry, TogglApiErrorResult> updateResult = await this.UpdateTimeEntryAsync(te, togglApiKey);

                if (updateResult.IsT1)
                {
                    return updateResult.AsT1;
                }

                responseTimeEntries.Add(updateResult.AsT0);
            }
            else
            {
                responseTimeEntries.Add(te);
            }
        }

        return responseTimeEntries;
    }
}