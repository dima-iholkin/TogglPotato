using OneOf;
using TogglPotato.WebAPI.Domain.AppService;
using TogglPotato.WebAPI.Domain.Validators.Errors;
using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries.Models;
using TogglPotato.WebAPI.HttpClients;
using TogglPotato.WebAPI.HttpClients.ErrorHandling;
using TogglPotato.WebAPI.HttpClients.ErrorHandling.Models;
using TogglPotato.WebAPI.Domain.Models;
using TogglPotato.WebAPI.ValidationErrors;
using TogglPotato.WebAPI.Validators;
using System.Diagnostics;

namespace TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries;

public class OrganizeDailyTimeEntriesEndpoint(
    DailyTimeEntriesOrganizer organizer, ITogglApiService togglHttpService, StartDateValidator startDateValidator,
    ActivitySource activitySource
)
{
    private TogglApiKey? _togglApiKey;
    private DateOnly _date;

    /// <summary>
    /// Handles the HTTP request, organizes the time entries in Toggl and returns the daily records in final order
    /// after the changes are done.
    /// </summary>
    public async Task<IResult> Handler(RequestBody request, CancellationToken cancellationToken)
    {
        using Activity? activity = activitySource.StartActivity(nameof(OrganizeDailyTimeEntriesEndpoint));

        // 1. Validate the input values.

        // 1.1 Validate the Toggl API key.

        OneOf<TogglApiKey, TogglApiKeyValidationError> togglApiKeyResult = TogglApiKey.ConvertFrom(
            request.TogglApiKey
        );

        if (togglApiKeyResult.IsT1)
        {
            return TypedResults.BadRequest(new { TogglApiKeyValidationError.Message });
        }

        this._togglApiKey = togglApiKeyResult.AsT0;

        // 1.2 Validate the date.

        if (request.Date == default(DateOnly))
        {
            return TypedResults.BadRequest(new { Message = "Please provide a date." });
        }

        if (InputDateValidator.ValidateInputDateIsWithinLast3Months(request.Date) == false)
        {
            string message =
                "Please provide a date within last 3 months. This is a requirement of the Toggl API used by this app.";
            return TypedResults.BadRequest(new { Message = message });
        }

        this._date = request.Date;

        // 2. Get the TimeZoneInfo.

        TimeZoneInfo? timezoneInfo;

        using (Activity? activity2 = activitySource.StartActivity("Get the TimeZoneInfo"))
        {
            // 2.1 Get the UserProfile.
            OneOf<UserProfile, TogglApiErrorResult> userProfileResult = await togglHttpService.GetUserProfileAsync(
                this._togglApiKey, cancellationToken
            );

            // 2.2 Deal with the Toggl API errors.
            if (userProfileResult.IsT1)
            {
                TogglApiErrorResult errorResult = userProfileResult.AsT1;
                IResult result = TogglApiErrorHandler.HandleTogglApiServiceErrors(errorResult);
                return result;
            }

            // 2.3 Get the correct TimeZoneInfo.
            timezoneInfo = userProfileResult.AsT0.TimeZoneInfo;

            // 2.4 Validate the start_time will be within the last 3 month.
            if (startDateValidator.ValidateStartDateIsWithinLast3Months(this._date, timezoneInfo) == false)
            {
                string message = "Please provide a date within last 3 months. " +
                    "This is a requirement of the Toggl API used by this app.";
                return TypedResults.BadRequest(new { Message = message });
            }
        }

        // 3.1 Get the daily time entries.

        List<TimeEntry>? originalTimeEntries;

        using (Activity? activity3 = activitySource.StartActivity("Get the daily time entries"))
        {
            OneOf<List<TimeEntry>, TogglApiErrorResult> timeEntriesResult =
                await togglHttpService.GetDailyTimeEntriesAsync(
                    timezoneInfo, this._date, this._togglApiKey, cancellationToken
                );

            // 3.2 Deal with the Toggl API errors.
            if (timeEntriesResult.IsT1)
            {
                TogglApiErrorResult errorResult = timeEntriesResult.AsT1;
                IResult result = TogglApiErrorHandler.HandleTogglApiServiceErrors(errorResult);
                return result;
            }

            // 3.3 Get the correct List<TimeEntry>.
            originalTimeEntries = timeEntriesResult.AsT0;

            // 3.4 Handle the case of no Time Entries on the given date.
            if (originalTimeEntries.Count == 0)
            {
                string notFoundMessage =
                    "There are no time entries on the given date. Please provide a date with time entries.";
                return TypedResults.NotFound(new { Message = notFoundMessage });
            }
        }

        // 4.1 Sort and prepare the time entries to modify at Toggl API.

        List<TimeEntry>? sortedTimeEntries;

        using (Activity? activity4 = activitySource.StartActivity(
            "Sort and prepare the time entries for modification"
        ))
        {
            OneOf<List<TimeEntry>, DailyTotalTimeExceedsFullDayValidationError> sortAndPrepareResult =
                organizer.SortAndModifyTimeEntries(originalTimeEntries, timezoneInfo, this._date, cancellationToken);

            // 4.2 Handle the potential errors.
            if (sortAndPrepareResult.IsT1)
            {
                return TypedResults.Problem(
                    detail: DailyTotalTimeExceedsFullDayValidationError.Message,
                    statusCode: StatusCodes.Status409Conflict
                );
            }

            // 4.3 Get the correct prepared TimeEntries.
            sortedTimeEntries = sortAndPrepareResult.AsT0;
        }

        // 5.1 Upload the modified time entries into Toggl and get the results.

        List<TimeEntry>? responseTimeEntries;

        using (Activity? activity5 = activitySource.StartActivity("Upload the modified time entries"))
        {
            OneOf<List<TimeEntry>, TogglApiErrorResult> uploadResult = await togglHttpService.UpdateTimeEntriesAsync(
                sortedTimeEntries, this._togglApiKey, cancellationToken
            );

            // 5.2 Deal with the Toggl API errors.
            if (uploadResult.IsT1)
            {
                TogglApiErrorResult errorResult = uploadResult.AsT1;
                IResult result = TogglApiErrorHandler.HandleTogglApiServiceErrors(errorResult);
                return result;
            }

            // 5.3 Get the correct modified Time Entries.
            responseTimeEntries = uploadResult.AsT0;
        }

        // 5.4 Return the correct modified Time Entries list.
        return TypedResults.Ok(responseTimeEntries);
    }
}