using TimeZoneConverter;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.Helpers;

public static class DateTimeHelper
{
    public static (DateTime startTime, DateTime endTime) GenerateUtcTimeForDailyTimeEntries(string timezone, DateTime date)
    {
        TimeSpan offset = DateTimeHelper.GetTimezoneOffset(timezone, date);

        DateTime _startTime = date.Subtract(offset);
        DateTime _endTime = date.AddDays(1).Subtract(offset).AddTicks(-1);

        return (_startTime, _endTime);
    }

    public static bool CheckDailyTimeIsUpTo24Hours(List<TimeEntry> timeEntries, ILogger logger)
    {
        TimeSpan totalTime = new TimeSpan();
        foreach (TimeEntry te in timeEntries)
        {
            totalTime =+ new TimeSpan(hours: 0, minutes: 0, seconds: te.Duration);
        }

        string timespanString = string.Format("{0}h{1}m{2}s",
            totalTime.Days * 24 + totalTime.Hours,
            totalTime.Minutes,
            totalTime.Seconds
        );
        logger.LogDebug("Total time of daily time entries: {TotalTime}.", timespanString);

        if (totalTime.TotalHours > 24)
        {
            return false;
        }

        return true;
    }

    public static TimeSpan GetTimezoneOffset(string timezone, DateTime date)
    {
        string timezoneWindowsString = TZConvert.IanaToWindows(timezone);

        TimeZoneInfo? timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneWindowsString);

        TimeSpan offset = timezoneInfo.GetUtcOffset(date);
        return offset;
    }
}