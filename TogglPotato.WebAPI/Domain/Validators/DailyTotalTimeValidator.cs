using TogglPotato.WebAPI.Domain.Services;
using TogglPotato.WebAPI.Domain.Models;

namespace TogglPotato.WebAPI.Domain.Validators;

public class DailyTotalTimeValidator(GlobalTimeService timeService, ILogger<DailyTotalTimeValidator> logger)
{
    public bool CheckTotalTimeDoesntExceedFullDay(List<TimeEntry> timeEntries, TimeZoneInfo tzInfo, DateOnly date)
    {
        // 1. Count the total time.

        TimeSpan totalTime = new TimeSpan();
        foreach (TimeEntry te in timeEntries)
        {
            totalTime += new TimeSpan(hours: 0, minutes: 0, seconds: te.Duration);
        }

        // 2. Log the total time.

        string timespanString = string.Format("{0} hours {1} minutes {2} seconds",
            Math.Floor(totalTime.TotalHours),
            totalTime.Minutes,
            totalTime.Seconds
        );
        logger.LogDebug("Total time of daily time entries: {TotalTime}.", timespanString);

        // 3. Return the boolean result.

        if (totalTime > timeService.GetDailyTimeSpan(tzInfo, date))
        {
            return false;
        }

        return true;
    }
}