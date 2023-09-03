using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OneOf;
using TogglPotato.WebAPI.Helpers;
using TogglPotato.WebAPI.HttpClients.Models;
using TogglPotato.WebAPI.HttpClients.TogglApiErrors;
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

    public async ValueTask<OneOf<UserProfile, TogglApiErrorResult>> GetUserProfile(TogglApiKey togglApiKey, ILogger logger)
    {
        // 1. Configure and send the Toggl API request.

        HttpResponseMessage? response;

        using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v9/me"))
        {
            requestMessage.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            requestMessage.Headers.AddApiKeyAuthorization(togglApiKey);

            response = await _httpClient.SendAsync(requestMessage);
        }

        // 2. Deal with the API error responses.

        if (response.StatusCode != HttpStatusCode.OK)
        {
            OneOf<
                TogglApiKeyError,
                TogglServerError,
                TooManyRequestsError,
                UnexpectedTogglApiError
            > togglApiError = response.StatusCode switch
            {
                // 400 error codes:
                HttpStatusCode.Forbidden => new TogglApiKeyError(),
                HttpStatusCode.TooManyRequests => new TooManyRequestsError(),
                // 500 error codes:
                HttpStatusCode.BadGateway => new TogglServerError(HttpStatusCode.BadGateway),
                HttpStatusCode.InternalServerError => new TogglServerError(HttpStatusCode.InternalServerError),
                HttpStatusCode.ServiceUnavailable => new TogglServerError(HttpStatusCode.ServiceUnavailable),
                HttpStatusCode.GatewayTimeout => new TogglServerError(HttpStatusCode.GatewayTimeout),
                // other codes codes:
                _ => new UnexpectedTogglApiError(response.StatusCode)
            };

            TogglApiErrorResult errorResult = new TogglApiErrorResult(togglApiError);

            logger.LogWarning(
                "Toggl API UserProfile request returned error with StatusCode {StatusCode}.",
                response.StatusCode.ToString()
            );
            logger.LogWarning("Toggl API error message: {ErrorMessage}", await response.Content.ReadAsStringAsync());

            return errorResult;
        }

        // 3. Deserialize the UserProfileResponse.

        UserProfileResponse? userProfileResponse = await response.Content.ReadFromJsonAsync<UserProfileResponse>();

        // TODO: Produce a proper return for deserialization error.
        if (userProfileResponse == null)
        {
            throw new Exception("UserProfileResponse wasn't deserialized correctly.");
        }

        // 4. Return the UserProfile on success.

        UserProfile userProfile = userProfileResponse.ConvertToUserProfile();
        return userProfile;
    }

    public async ValueTask<List<TimeEntry>> GetDailyTimeEntries(
        TimeZoneInfo timezoneInfo, DateTime date, TogglApiKey togglApiKey, ILogger logger
    )
    {
        // 1. Generate the start and end times:

        (DateTime startTime, DateTime endTime) = DateTimeHelper.GenerateUtcTimeForDailyTimeEntries(
            timezoneInfo, date
        );

        // 2. Configure and send a request to Toggl API:

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

            response = await _httpClient.SendAsync(requestMessage);
        }

        // 3. Deal with the error HTTP responses:

        // TODO: Do a proper Toggl API error handling.

        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogWarning(
                "Toggl daily time entries request returned StatusCode {StatusCode}.",
                response.StatusCode.ToString()
            );
            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        // 4. Return the List<TimeEntry> on success:

        // TODO: Why return new empty List, if the content wasn't deserialized correctly?
        List<TimeEntry> timeEntries = await response.Content.ReadFromJsonAsync<List<TimeEntry>>()
            ?? new List<TimeEntry>();

        timeEntries.ForEach(te =>
        {
            te.Start = te.Start.ToUniversalTime();
        });

        return timeEntries;
    }

    public async ValueTask<TimeEntry> UpdateTimeEntry(
        TimeEntry timeEntry, TogglApiKey togglApiKey, ILogger logger
    )
    {
        HttpResponseMessage? response;

        // 1. Configure and send a request to Toggl API.

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

            requestMessage.Headers.AddApiKeyAuthorization(togglApiKey);

            response = _httpClient.Send(requestMessage);
        }

        // 2. Deal with the error HTTP responses.

        if (response.IsSuccessStatusCode == false)
        {
            logger.LogWarning("HTTP response error code: {ErrorCode}", response.StatusCode);
            logger.LogWarning(
                "HTTP response error message: {ErrorMessage}", await response.Content.ReadAsStringAsync()
            );
        }

        // 3. Return the updated TimeEntry on success.

        TimeEntry teResponse = await response.Content.ReadFromJsonAsync<TimeEntry>()
            ?? throw new NotImplementedException();
        return teResponse;
    }
}