using OneOf;
using TogglPotato.WebAPI.HttpClients.ErrorHandling.Models;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.HttpClients;

public interface ITogglHttpService
{
    ValueTask<OneOf<UserProfile, TogglApiErrorResult>> GetUserProfileAsync(TogglApiKey apiKey);

    ValueTask<OneOf<List<TimeEntry>, TogglApiErrorResult>> GetDailyTimeEntriesAsync(
        TimeZoneInfo timezoneInfo, DateTime date, TogglApiKey apiKey
    );

    Task<OneOf<List<TimeEntry>, TogglApiErrorResult>> UpdateTimeEntriesAsync(
        List<TimeEntry> timeEntries, TogglApiKey togglApiKey
    );
}