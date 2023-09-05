namespace TogglPotato.WebAPI.Helpers;

public static class DateTimeHelper
{
    // TODO: Probably should be placed somewhere in business logic.
    public static (DateTime startTime, DateTime endTime) GenerateUtcTimeRangeForDailyTimeEntries(
        TimeZoneInfo userTimezone, DateTime date
    )
    {
        TimeSpan offset = userTimezone.GetTimeZoneOffset(date);

        DateTime _startTime = date.Subtract(offset);
        DateTime _endTime = date.AddDays(1).Subtract(offset).AddTicks(-1);

        return (_startTime, _endTime);
    }
}