using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TogglPotato.WebAPI.Helpers;
using TogglPotato.WebAPI.HttpClients;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries;

public class OrganizeDailyTimeEntries_Endpoint
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
        ILogger<OrganizeDailyTimeEntries_Endpoint> logger
    )
    {
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

        TimeSpan dailyTimeCursor = new TimeSpan();
        TimeSpan utcOffset = DateTimeHelper.GetTimezoneOffset(userProfile.Timezone, requestBody.Date);

        List<TimeEntry> responseTimeEntries = new List<TimeEntry>();

        sortedTimeEntries.ForEach(async (te) =>
        {
            DateTime newStartTime = requestBody.Date.Add(dailyTimeCursor).Subtract(utcOffset);

            if (te.Start != newStartTime)
            {
                te.Start = newStartTime;
                te.Stop = newStartTime.AddSeconds(te.Duration);

                TimeEntry teResponse = await togglHttpService.UpdateTimeEntry(te, requestBody.ApiKey, logger);

                responseTimeEntries.Add(teResponse);
            }
            else
            {
                responseTimeEntries.Add(te);
            }

            dailyTimeCursor = dailyTimeCursor.Add(new TimeSpan(hours: 0, minutes: 0, seconds: te.Duration));
        });

        return TypedResults.Ok(responseTimeEntries);
    }
}