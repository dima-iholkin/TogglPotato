using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.HttpClients.Models;

public class UserProfileResponse
{
    public string Timezone { get; init; } = "";

    public UserProfile ConvertToUserProfile()
    {
        return new UserProfile(this.Timezone);
    }
}