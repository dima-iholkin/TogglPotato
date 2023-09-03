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
}