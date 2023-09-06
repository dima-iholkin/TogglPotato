using OneOf;
using TogglPotato.WebAPI.Domain.Services;
using TogglPotato.WebAPI.Domain.Validators;
using TogglPotato.WebAPI.Domain.Validators.Errors;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.Domain.AppService;

public class DailyTimeEntriesOrganizer(GlobalTimeService timeService, DailyTotalTimeValidator dailyTotalTimeValidator)
{
    public OneOf<List<TimeEntry>, DailyTotalTimeExceedsFullDayValidationError> SortAndModifyTimeEntries(
        List<TimeEntry> timeEntries, TimeZoneInfo tzInfo, DateOnly date
    )
    {
        // 1. Check the total time does not exceed a full day.

        bool totalTimeDoesntExceedFullDay = dailyTotalTimeValidator.CheckTotalTimeDoesntExceedFullDay(
            timeEntries, tzInfo, date
        );

        if (totalTimeDoesntExceedFullDay == false)
        {
            return new DailyTotalTimeExceedsFullDayValidationError();
        }

        // 2. Sort the Time Entries.

        List<TimeEntry> sortedTimeEntries = timeEntries.OrderBy(te => te.Start).ThenBy(te => te.Id).ToList();

        // 3. Modify the Time Entries before an upload.

        (DateTime startDateUtc, _) = timeService.GenerateUtcTimeRangeForDailyTimeEntries(tzInfo, date);

        TimeSpan dailyTimeCount = new TimeSpan();

        sortedTimeEntries.ForEach(te =>
        {
            DateTime newStartTime = startDateUtc.Add(dailyTimeCount);

            if (te.Start != newStartTime)
            {
                te.Start = newStartTime;
                te.Stop = newStartTime.AddSeconds(te.Duration);

                te.Modified = true;
            }

            dailyTimeCount += new TimeSpan(hours: 0, minutes: 0, seconds: te.Duration);
        });

        // 4. Return the modified Time Entries.

        return sortedTimeEntries;
    }
}