using System.Net;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries.Models;
using TogglPotato.WebAPI.Helpers;
using TogglPotato.WebAPI.HttpClients;
using TogglPotato.WebAPI.HttpClients.Models;
using TogglPotato.WebAPI.HttpClients.TogglApiErrors;
using TogglPotato.WebAPI.Models;
using TogglPotato.WebAPI.ValidationErrors;
using TogglPotato.WebAPI.Validators;

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

    // TODO: It's questionable to have this data as class fields.
    private TogglApiKey? _togglApiKey;
    private DateTime _date;

    /// <summary>
    /// Handles the HTTP request, organizes the time entries in Toggl and returns the daily records in final order
    /// after the changes are done.
    /// </summary>
    public async Task<IResult> Handler(RequestBody requestBody)
    {
        // 1. Validate the input values:

        // 1.1 Validate the Toggl API key.

        OneOf<TogglApiKey, TogglApiKeyValidationError> togglApiKeyResult = TogglApiKey.ConvertFrom(
            requestBody.TogglApiKey
        );

        if (togglApiKeyResult.IsT1)
        {
            return TypedResults.BadRequest(new { TogglApiKeyValidationError.Message });
        }

        _togglApiKey = togglApiKeyResult.AsT0;

        // 1.2 Validate the date.

        if (requestBody.Date == default(DateTime))
        {
            return TypedResults.BadRequest(new { Message = "Please provide a date." });
        }

        if (InputDateValidator.Validate(requestBody.Date) == false)
        {
            return TypedResults.BadRequest(new { Message = "Please provide date without a time." });
        }

        _date = requestBody.Date;

        // 2.1. Get the user's timezone from UserProfile:

        OneOf<UserProfile, TogglApiErrorResult> userProfileResult = await togglHttpService.GetUserProfile(
            _togglApiKey, logger
        );

        // 2.2. Deal with the Toggl API errors:

        if (userProfileResult.IsT1)
        {
            var error = userProfileResult.AsT1.Error;

            IResult result = error.Match<IResult>(
                (TogglApiKeyError togglApiError) => TypedResults.BadRequest(new { TogglApiKeyError.Message }),
                (TogglServerError togglServerError) => Results.Json(
                    new { togglServerError.Message }, statusCode: (int)HttpStatusCode.InternalServerError
                ),
                (TooManyRequestsError tooManyRequest) => Results.Json(
                    new { TooManyRequestsError.Message }, statusCode: (int)HttpStatusCode.TooManyRequests
                ),
                (UnexpectedTogglApiError unexpectedTogglApiError) => Results.Json(
                    new { unexpectedTogglApiError.Message }, statusCode: (int)HttpStatusCode.InternalServerError
                )
            );

            return result;
        }

        // 2.3 Get the correct TimeZoneInfo.

        TimeZoneInfo timezoneInfo = userProfileResult.AsT0.TimeZoneInfo;

        // 3.1. Get the daily time entries:

        List<TimeEntry> originalTimeEntries = await togglHttpService.GetDailyTimeEntries(
            timezoneInfo, this._date, _togglApiKey!, logger
        );

        // 3.2. Deal with the Toggl API errors:

        // TODO: Handle the potential Toggl API errors.

        // 4. Sort and prepare the time entries to modify in Toggl:

        OneOf<List<TimeEntry>, DailyTotalTimeExceedsFullDayValidationError> sortAndPrepareResult =
            SortAndPrepareTimeEntries(originalTimeEntries, timezoneInfo);

        // 5.1. Handle the potential errors:

        if (sortAndPrepareResult.IsT1)
        {
#pragma warning disable IDE0059 // Unnecessary assignment
            DailyTotalTimeExceedsFullDayValidationError totalTimeExceedsFullDay = sortAndPrepareResult.AsT1;
#pragma warning restore IDE0059

            return TypedResults.Problem(
                detail: DailyTotalTimeExceedsFullDayValidationError.Message,
                statusCode: StatusCodes.Status409Conflict
            );
        }

        // 5.2. Get the correct prepared TimeEntries.

        List<TimeEntry> sortedTimeEntries = sortAndPrepareResult.AsT0;

        // 6.1. Upload the time entries into Toggl and return the results.

        List<TimeEntry> responseTimeEntries = await UploadTimeEntriesAsync(sortedTimeEntries);

        // 6.2 Handle the Toggl API errors.

        // TODO: Implement it.

        // 6.3 Return the correct modified List of Time Entries.

        return TypedResults.Ok(responseTimeEntries);
    }

    // Helper methods:

    private OneOf<List<TimeEntry>, DailyTotalTimeExceedsFullDayValidationError> SortAndPrepareTimeEntries(
        List<TimeEntry> timeEntries, TimeZoneInfo timezoneInfo
    )
    {
        // 1. Check the daily total time:

        bool totalTimeDoesntExceedFullDay = DateTimeHelper.CheckTotalTimeDoesntExceedFullDay(timeEntries, logger);

        if (totalTimeDoesntExceedFullDay == false)
        {
            return new DailyTotalTimeExceedsFullDayValidationError();
        }

        // 2. Sort the time entries:

        List<TimeEntry> sortedTimeEntries = timeEntries.OrderBy(te => te.Start).ThenBy(te => te.Id).ToList();

        // 3. Modify the time entries for upload:

        TimeSpan dailyTimeCount = new TimeSpan();
        TimeSpan utcOffset = DateTimeHelper.GetTimezoneOffset(timezoneInfo, _date);

        sortedTimeEntries.ForEach(te =>
        {
            DateTime newStartTime = _date.Add(dailyTimeCount).Subtract(utcOffset);

            if (te.Start != newStartTime)
            {
                te.Start = newStartTime;
                te.Stop = newStartTime.AddSeconds(te.Duration);

                te.Modified = true;
            }

            dailyTimeCount = dailyTimeCount.Add(new TimeSpan(hours: 0, minutes: 0, seconds: te.Duration));
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