using TogglPotato.WebAPI.Domain.Services;

namespace TogglPotato.WebAPI.Validators;

public class StartDateValidator(GlobalTimeService timeService)
{
    public bool ValidateStartDateIsWithinLast3Months(DateOnly date, TimeZoneInfo tzInfo)
    {
        (DateTime startDateTimeUtc, _) = timeService.GenerateUtcTimeRangeForDailyTimeEntries(tzInfo, date);
        DateOnly startDate = DateOnly.FromDateTime(startDateTimeUtc);

        DateTime dateTime3MonthsAgo = DateTime.UtcNow.AddMonths(-3);
        DateOnly date3MonthsAgo = DateOnly.FromDateTime(dateTime3MonthsAgo);

        // If the start_date is older than 3 months ago, the Toggl GET Time Entries API wouldn't work.
        if (startDate < date3MonthsAgo)
        {
            return false;
        }

        return true;
    }
}