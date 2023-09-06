using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.Domain.Validators;

public class DailyTotalTimeValidator(ILogger<DailyTotalTimeValidator> logger)
{
    public bool CheckTotalTimeDoesntExceedFullDay(List<TimeEntry> timeEntries)
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