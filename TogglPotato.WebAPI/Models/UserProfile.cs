using TimeZoneConverter;

namespace TogglPotato.WebAPI.Models;

public class UserProfile
{
    // Init:

    // TODO: Deal with the potential unknown timezone names and so on.
    public UserProfile(string timezone)
    {
        if (String.IsNullOrEmpty(timezone))
        {
            throw new ArgumentException("timezone is null or empty", nameof(timezone));
        }

        string windowsTimezoneName = TZConvert.IanaToWindows(timezone);

        TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(windowsTimezoneName);
    }

    // Properties:

    public TimeZoneInfo TimeZoneInfo { get; init; }

    // Deconstructors:

    public void Deconstruct(out TimeZoneInfo tzInfo, out string? _)
    {
        tzInfo = TimeZoneInfo;
        _ = null;
    }
}