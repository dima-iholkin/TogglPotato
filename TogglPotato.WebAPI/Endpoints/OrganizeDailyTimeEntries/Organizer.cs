using OneOf;
using TogglPotato.WebAPI.Domain.Validators;
using TogglPotato.WebAPI.Domain.Validators.Errors;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries;

public class Organizer(DailyTotalTimeValidator dailyTotalTimeValidator)
{
    public OneOf<List<TimeEntry>, DailyTotalTimeExceedsFullDayValidationError> SortAndPrepareTimeEntries(
        List<TimeEntry> timeEntries, TimeZoneInfo timezoneInfo, DateOnly date
    )
    {
        // 1. Check the total time does not exceed a full day.

        bool totalTimeDoesntExceedFullDay = dailyTotalTimeValidator.CheckTotalTimeDoesntExceedFullDay(timeEntries);

        if (totalTimeDoesntExceedFullDay == false)
        {
            return new DailyTotalTimeExceedsFullDayValidationError();
        }

        // 2. Sort the Time Entries.

        List<TimeEntry> sortedTimeEntries = timeEntries.OrderBy(te => te.Start).ThenBy(te => te.Id).ToList();

        // 3. Modify the Time Entries before an upload.

        TimeSpan dailyTimeCount = new TimeSpan();

        DateTime dateTime = new DateTime(date, TimeOnly.MinValue);
        TimeSpan utcOffset = timezoneInfo.GetUtcOffset(dateTime);

        sortedTimeEntries.ForEach(te =>
        {
            DateTime newStartTime = dateTime.Add(dailyTimeCount).Subtract(utcOffset);

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