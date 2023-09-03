using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.Helpers;

public static class DateTimeHelper
{
    public static TimeSpan GetTimezoneOffset(TimeZoneInfo userTimezone, DateTime date)
    {
        TimeSpan offset = userTimezone.GetUtcOffset(date);
        return offset;
    }

    public static (DateTime startTime, DateTime endTime) GenerateUtcTimeForDailyTimeEntries(
        TimeZoneInfo userTimezone, DateTime date
    )
    {
        TimeSpan offset = DateTimeHelper.GetTimezoneOffset(userTimezone, date);

        DateTime _startTime = date.Subtract(offset);
        DateTime _endTime = date.AddDays(1).Subtract(offset).AddTicks(-1);

        return (_startTime, _endTime);
    }

    public static bool CheckTotalTimeDoesntExceedFullDay(List<TimeEntry> timeEntries, ILogger logger)
    {
        // 1. Count the total time.

        TimeSpan totalTime = new TimeSpan();
        foreach (TimeEntry te in timeEntries)
        {
            totalTime += new TimeSpan(hours: 0, minutes: 0, seconds: te.Duration);
        }

        // 2. Log the total time.

        string timespanString = string.Format("{0}h{1}m{2}s",
            totalTime.Days * 24 + totalTime.Hours,
            totalTime.Minutes,
            totalTime.Seconds
        );
        logger.LogDebug("Total time of daily time entries: {TotalTime}.", timespanString);

        // 3. Return the boolean result.

        if (totalTime.TotalHours > 24)
        {
            return false;
        }

        return true;
    }
}