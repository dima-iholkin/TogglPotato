using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries.ValidationErrors;
using TogglPotato.WebAPI.Helpers;
using TogglPotato.WebAPI.HttpClients;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries;

public class OrganizeDailyTimeEntriesEndpoint(
    ITogglHttpService togglHttpService,
    ILogger<OrganizeDailyTimeEntriesEndpoint> logger
)
{
    public static void Map(WebApplication app)
    {
        app.MapPost(
            "/api/organize_daily_time_entries",
            ([FromBody] RequestBody requestBody, [FromServices] OrganizeDailyTimeEntriesEndpoint endpoint) =>
                endpoint.Handler(requestBody)
        );
    }

    private string? _togglApiKey;
    private DateTime _date;

    /// <summary>
    /// Handles the HTTP request, organizes the time entries in Toggl and returns the daily records in final order
    /// after the changes are done.
    /// </summary>
    public async Task<Results<Ok<List<TimeEntry>>, ProblemHttpResult>> Handler(
        RequestBody requestBody
    )
    {
        (_togglApiKey, _date) = requestBody;

        // 1. Get the daily time entries:

        (List<TimeEntry> originalTimeEntries, string userTimezone) = await GetDailyTimeEntriesAndUserTimezoneAsync();

        // 2. Sort and prepare the time entries to modify in Toggl:

        var sortAndPreparationResult = SortAndPrepareTimeEntries(originalTimeEntries, userTimezone);

        // 2.1. Handle the potential errors:

        if (sortAndPreparationResult.IsT1)
        {
#pragma warning disable IDE0059 // Unnecessary assignment
            TotalTimeExceedsFullDayValidationError totalTimeExceedsFullDay = sortAndPreparationResult.AsT1;
#pragma warning restore IDE0059

            return TypedResults.Problem(
                detail: TotalTimeExceedsFullDayValidationError.Message,
                statusCode: StatusCodes.Status409Conflict
            );
        }

        // 2.2. Convert the OneOf result to the correct happy path type:

        if (sortAndPreparationResult.IsT0 == false)
        {
            throw new Exception("OneOf result type is wrong.");
        }

        List<TimeEntry> sortedTimeEntries = sortAndPreparationResult.AsT0;

        // 3. Upload the time entries into Toggl and return the results.

        List<TimeEntry> responseTimeEntries = await UploadTimeEntriesAsync(sortedTimeEntries);

        return TypedResults.Ok(responseTimeEntries);
    }

    // Helper methods:

    private async Task<(List<TimeEntry> timeEntries, string userTimezone)> GetDailyTimeEntriesAndUserTimezoneAsync()
    {
        (string userTimezone, _) = await togglHttpService.GetUserProfile(_togglApiKey!, logger);

        (DateTime startTime, DateTime endTime) = DateTimeHelper.GenerateUtcTimeForDailyTimeEntries(userTimezone, _date);

        List<TimeEntry> timeEntries = await togglHttpService.GetDailyTimeEntries(
            startTime, endTime, _togglApiKey!, logger
        );

        timeEntries.ForEach(te =>
        {
            te.Start = te.Start.ToUniversalTime();
        });

        return (timeEntries, userTimezone);
    }

    private OneOf<
        List<TimeEntry>, TotalTimeExceedsFullDayValidationError
    > SortAndPrepareTimeEntries(List<TimeEntry> timeEntries, string userTimezone)
    {
        // Check the total time:

        bool totalTimeUpTo24Hours = DateTimeHelper.CheckDailyTimeIsUpTo24Hours(timeEntries, logger);

        if (totalTimeUpTo24Hours == false)
        {
            return new TotalTimeExceedsFullDayValidationError();
            // return TypedResults.Problem(
            //     detail: "Total daily time of time entries was above 24 hours.",
            //     statusCode: StatusCodes.Status409Conflict
            // );
        }

        // Sort the time entries:

        List<TimeEntry> sortedTimeEntries = timeEntries.OrderBy(te => te.Start).ThenBy(te => te.Id).ToList();

        // Prepare the time entries for upload:

        TimeSpan dailyTimeCursor = new TimeSpan();
        TimeSpan utcOffset = DateTimeHelper.GetTimezoneOffset(userTimezone, _date);

        List<TimeEntry> responseTimeEntries = new List<TimeEntry>();

        sortedTimeEntries.ForEach(te =>
        {
            DateTime newStartTime = _date.Add(dailyTimeCursor).Subtract(utcOffset);

            if (te.Start != newStartTime)
            {
                te.Start = newStartTime;
                te.Stop = newStartTime.AddSeconds(te.Duration);

                te.Modified = true;
            }

            dailyTimeCursor = dailyTimeCursor.Add(new TimeSpan(hours: 0, minutes: 0, seconds: te.Duration));
        });

        return sortedTimeEntries;
    }

    public async Task<List<TimeEntry>> UploadTimeEntriesAsync(List<TimeEntry> sortedTimeEntries)
    {
        List<TimeEntry> responseTimeEntries = new List<TimeEntry>();

        foreach (TimeEntry te in sortedTimeEntries)
        {
            if (te.Modified == true)
            {
                TimeEntry teResponse = await togglHttpService.UpdateTimeEntry(te, _togglApiKey!, logger);
                responseTimeEntries.Add(teResponse);
            }
            else
            {
                responseTimeEntries.Add(te);
            }
        }

        return responseTimeEntries;
    }
}