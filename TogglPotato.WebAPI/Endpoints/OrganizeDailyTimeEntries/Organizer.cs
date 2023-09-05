using OneOf;
using TogglPotato.WebAPI.Helpers;
using TogglPotato.WebAPI.Models;
using TogglPotato.WebAPI.ValidationErrors;
using TogglPotato.WebAPI.Validators;

namespace TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries;

public class Organizer(ILogger<Organizer> logger)
{
    public OneOf<List<TimeEntry>, DailyTotalTimeExceedsFullDayValidationError> SortAndPrepareTimeEntries(
        List<TimeEntry> timeEntries, TimeZoneInfo timezoneInfo, DateTime date
    )
    {
        // 1. Check the total time does not exceed a full day.

        bool totalTimeDoesntExceedFullDay = TotalTimeValidator.CheckTotalTimeDoesntExceedFullDay(timeEntries, logger);

        if (totalTimeDoesntExceedFullDay == false)
        {
            return new DailyTotalTimeExceedsFullDayValidationError();
        }

        // 2. Sort the Time Entries.

        List<TimeEntry> sortedTimeEntries = timeEntries.OrderBy(te => te.Start).ThenBy(te => te.Id).ToList();

        // 3. Modify the Time Entries before an upload.

        TimeSpan dailyTimeCount = new TimeSpan();
        TimeSpan utcOffset = timezoneInfo.GetTimeZoneOffset(date);

        sortedTimeEntries.ForEach(te =>
        {
            DateTime newStartTime = date.Add(dailyTimeCount).Subtract(utcOffset);

            if (te.Start != newStartTime)
            {
                te.Start = newStartTime;
                te.Stop = newStartTime.AddSeconds(te.Duration);

                te.Modified = true;
            }

            dailyTimeCount = dailyTimeCount.Add(new TimeSpan(hours: 0, minutes: 0, seconds: te.Duration));
        });

        // 4. Return the modified Time Entries.

        return sortedTimeEntries;
    }
}