using OneOf;
using TogglPotato.WebAPI.Domain.Models;
using TogglPotato.WebAPI.HttpClients.ErrorHandling.Models;

namespace TogglPotato.WebAPI.HttpClients;

public interface ITogglApiService
{
    Task<OneOf<UserProfile, TogglApiErrorResult>> GetUserProfileAsync(
        TogglApiKey togglApiKey, CancellationToken cancellationToken
    );

    Task<OneOf<List<TimeEntry>, TogglApiErrorResult>> GetDailyTimeEntriesAsync(
        TimeZoneInfo tzInfo, DateOnly date, TogglApiKey apiKey, CancellationToken cancellationToken
    );

    Task<OneOf<List<TimeEntry>, TogglApiErrorResult>> UpdateTimeEntriesAsync(
        List<TimeEntry> timeEntries, TogglApiKey togglApiKey, CancellationToken cancellationToken
    );
}