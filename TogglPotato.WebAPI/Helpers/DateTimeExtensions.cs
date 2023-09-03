namespace TogglPotato.WebAPI.Helpers;

public static class DateTimeExtensions
{
    public static string ToTogglApiString(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.ff'Z'");
    }
}