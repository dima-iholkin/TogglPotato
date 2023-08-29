using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using TogglPotato.WebAPI.Helpers;
using TogglPotato.WebAPI.HttpClients;
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
        ITogglHttpService togglHttpService,
        ILogger<OrganizeTheDailyTimeEntries_Endpoint> logger
    )
    {
        // HttpClient httpClient = httpClientFactory.CreateClient("toggl_api");

        UserProfile userProfile = await togglHttpService.GetUserProfile(requestBody.ApiKey, logger);

        (DateTime startTime, DateTime endTime) = DateTimeHelper.GenerateUtcTimeForDailyTimeEntries(
            userProfile.Timezone,
            requestBody.Date
        );

        List<TimeEntry> timeEntries = await togglHttpService.GetDailyTimeEntries(
            startTime,
            endTime,
            requestBody.ApiKey,
            logger
        );

        // if (timeEntries.Count == 0)
        // {
        //     // logger.LogDebug(
        //     //     "Toggl API responded with an HttpCode {HttpCode} to Endpoint {Endpoint} request on Date {Date}.",
        //     //     response.StatusCode,
        //     //     "/me/time_entries",
        //     //     requestBody.Date
        //     // );

        //     return TypedResults.Problem(
        //     // detail: await response.Content.ReadAsStringAsync(),
        //     // statusCode: (int)response.StatusCode
        //     );
        // }

        bool totalTimeUpTo24Hours = DateTimeHelper.CheckDailyTimeIsUpTo24Hours(timeEntries, logger);

        if (totalTimeUpTo24Hours == false)
        {
            return TypedResults.Problem(
                detail: "Total daily time of time entries was above 24 hours.",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        List<TimeEntry> sortedTimeEntries = timeEntries.OrderBy(te => te.Start).ToList();

        sortedTimeEntries.ForEach(te => te.Start = te.Start.ToUniversalTime());

        return TypedResults.Ok(sortedTimeEntries);
    }
}

public record RequestBody(string ApiKey, DateTime Date);