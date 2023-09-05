using TogglPotato.WebAPI.Helpers;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.HttpClients.Models;

public class UserProfileResponse
{
    public string Timezone { get; init; } = "";

    public UserProfile ConvertToUserProfile(TimeZoneHelper tzHelper)
    {
        TimeZoneInfo tzInfo = tzHelper.FindTimeZoneFromToggl(this.Timezone);
        return new UserProfile(tzInfo);
    }
}