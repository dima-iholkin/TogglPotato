using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.Endpoints.OrganizeTheDailyTimeEntries;

public class OrganizeTheDailyTimeEntries_Endpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost(
            "/api/organize_the_daily_time_entries",
            Handler
        );
    }

    public static async Task<Results<Ok<List<TimeEntry>>, ProblemHttpResult>> Handler(
        [FromBody] RequestBody requestBody,
        HttpContext httpContext,
        IHttpClientFactory httpClientFactory,
        ILogger<OrganizeTheDailyTimeEntries_Endpoint> logger
    )
    {
        HttpClient httpClient = httpClientFactory.CreateClient("toggl_api");

        Dictionary<string, string?> queryString = new Dictionary<string, string?>()
        {
            { "start_date", requestBody.Date.ToString("yyyy-MM-dd") },
            { "end_date", requestBody.Date.AddDays(1).ToString("yyyy-MM-dd") }
        };
        string uri = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString("/api/v9/me/time_entries", queryString);
        using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
        {
            // requestMessage.Headers.Add("Content-Type", "application/json");
            requestMessage.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            string basicToken = Convert.ToBase64String(
                ASCIIEncoding.ASCII.GetBytes(
                    $"{requestBody.ApiKey}:api_token"
                )
            );
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicToken);

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                List<TimeEntry> timeEntries = await response.Content.ReadFromJsonAsync<List<TimeEntry>>()
                    ?? new List<TimeEntry>();

                // logger.LogDebug(
                //     "Time Entries on Date {Date}: \r\n {TimeEntries}",
                //     requestBody.Date,
                //     timeEntries
                // );

                List<TimeEntry> sortedTimeEntries = timeEntries.OrderBy(te => te.Start).ToList();

                TimeSpan timeSum = new TimeSpan();
                foreach (TimeEntry te in sortedTimeEntries)
                {
                    timeSum = timeSum + new TimeSpan(hours: 0, minutes: 0, seconds: te.Duration);
                }

                string timespanString = string.Format("{0}h{1}m{2}s",
                    timeSum.Days * 24 + timeSum.Hours,
                    timeSum.Minutes,
                    timeSum.Seconds
                );
                logger.LogDebug("Total time of daily time entries: {TimeSum}.", timespanString);

                return TypedResults.Ok(sortedTimeEntries);
            }
            else
            {
                logger.LogDebug(
                    "Toggl API responded with an HttpCode {HttpCode} to Endpoint {Endpoint} request on Date {Date}.",
                    response.StatusCode,
                    "/me/time_entries",
                    requestBody.Date
                );

                return TypedResults.Problem(
                    detail: await response.Content.ReadAsStringAsync(),
                    statusCode: (int)response.StatusCode
                );
            }
        }
    }
}

public record RequestBody(string ApiKey, DateOnly Date);