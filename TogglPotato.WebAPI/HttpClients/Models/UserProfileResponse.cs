using TogglPotato.WebAPI.Domain.Services;
using TogglPotato.WebAPI.Domain.Models;

namespace TogglPotato.WebAPI.HttpClients.Models;

public class UserProfileResponse
{
    public string Timezone { get; init; } = "";

    public UserProfile ConvertToUserProfile(GlobalTimeService timeService)
    {
        TimeZoneInfo tzInfo = timeService.FindTimeZoneFromTogglString(this.Timezone);
        return new UserProfile(tzInfo);
    }
}