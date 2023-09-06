using OneOf;
using TogglPotato.WebAPI.HttpClients.ErrorHandling.Models;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.HttpClients;

public interface ITogglApiService
{
    ValueTask<OneOf<UserProfile, TogglApiErrorResult>> GetUserProfileAsync(TogglApiKey togglApiKey);

    ValueTask<OneOf<List<TimeEntry>, TogglApiErrorResult>> GetDailyTimeEntriesAsync(
        TimeZoneInfo tzInfo, DateOnly date, TogglApiKey apiKey
    );

    Task<OneOf<List<TimeEntry>, TogglApiErrorResult>> UpdateTimeEntriesAsync(
        List<TimeEntry> timeEntries, TogglApiKey togglApiKey
    );
}