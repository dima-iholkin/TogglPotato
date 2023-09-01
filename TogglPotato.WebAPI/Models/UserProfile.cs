namespace TogglPotato.WebAPI.Models;

public class UserProfile()
{
    public string Timezone { get; set; } = "";

    public void Deconstruct(out string timezone, out string? _)
    {
        timezone = Timezone;
        _ = null;
    }
}