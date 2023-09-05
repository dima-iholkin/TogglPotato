namespace TogglPotato.WebAPI.Models;

public class UserProfile
{
    // Constructors:

    public UserProfile(TimeZoneInfo tzInfo)
    {
        TimeZoneInfo = tzInfo;
    }

    // Properties:

    public TimeZoneInfo TimeZoneInfo { get; init; }
}