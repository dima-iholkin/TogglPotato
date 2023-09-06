using OneOf;
using TogglPotato.WebAPI.HttpClients.ErrorHandling.Models;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.HttpClients;

public interface ITogglApiService
{
    ValueTask<OneOf<UserProfile, TogglApiErrorResult>> GetUserProfileAsync(
        TogglApiKey togglApiKey, CancellationToken cancellationToken
    );

    ValueTask<OneOf<List<TimeEntry>, TogglApiErrorResult>> GetDailyTimeEntriesAsync(
        TimeZoneInfo tzInfo, DateOnly date, TogglApiKey apiKey, CancellationToken cancellationToken
    );

    Task<OneOf<List<TimeEntry>, TogglApiErrorResult>> UpdateTimeEntriesAsync(
        List<TimeEntry> timeEntries, TogglApiKey togglApiKey, CancellationToken cancellationToken
    );
}