using TogglPotato.WebAPI.Models;
using TimeZoneConverter;

namespace TogglPotato.WebAPI.Helpers;

public static class DateTimeHelper
{
    public static (DateTime startTime, DateTime endTime) GenerateUtcTimeForDailyTimeEntries(string timezone, DateTime date)
    {
        TimeZoneInfo? tzInfo;

        // if (timezone == "Europe/Kyiv")
        // {
        //     tzInfo = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kiev");
        // }

        string tzString = TZConvert.IanaToWindows(timezone);

        tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzString);

        TimeSpan offset = tzInfo.GetUtcOffset(date);

        DateTime _startTime = date.Subtract(offset);
        DateTime _endTime = date.AddDays(1).Subtract(offset).AddTicks(-1);

        return (_startTime, _endTime);
    }

    public static bool CheckDailyTimeIsUpTo24Hours(List<TimeEntry> timeEntries, ILogger logger)
    {
        TimeSpan totalTime = new TimeSpan();
        foreach (TimeEntry te in timeEntries)
        {
            totalTime = totalTime + new TimeSpan(hours: 0, minutes: 0, seconds: te.Duration);
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
}