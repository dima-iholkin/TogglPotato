using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

    // public static async Task<Results<Ok<List<TimeEntry>, BadRequest>>> Handler(
    public static async Task<Results<Ok<List<TimeEntry>>, ProblemHttpResult>> Handler(
        [FromBody] RequestBody requestBody,
        HttpContext httpContext,
        IHttpClientFactory httpClientFactory,
        ILogger<OrganizeTheDailyTimeEntries_Endpoint> logger
    )
    {
        HttpClient httpClient = httpClientFactory.CreateClient("toggl_api");

        using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/me/time_entries"))
        {
            requestMessage.Headers.Add("Content-Type", "application/json");

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
                logger.LogDebug(
                    "Time Entries on Date {Date}: \r\n {TimeEntries}",
                    requestBody.Date,
                    timeEntries
                );

                return TypedResults.Ok(timeEntries);
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