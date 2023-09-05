namespace TogglPotato.WebAPI.Helpers;

public static class TimeZoneInfoExtensions
{
    public static TimeSpan GetTimeZoneOffset(this TimeZoneInfo timeZone, DateTime date)
    {
        TimeSpan offset = timeZone.GetUtcOffset(date);
        return offset;
    }
}