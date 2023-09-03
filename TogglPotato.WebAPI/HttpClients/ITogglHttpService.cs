using OneOf;
using TogglPotato.WebAPI.HttpClients.Models;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.HttpClients;

public interface ITogglHttpService
{
    ValueTask<OneOf<UserProfile, TogglApiErrorResult>> GetUserProfile(TogglApiKey apiKey, ILogger logger);

    ValueTask<List<TimeEntry>> GetDailyTimeEntries(
        TimeZoneInfo timezoneInfo, DateTime date, TogglApiKey apiKey, ILogger logger
    );

    ValueTask<TimeEntry> UpdateTimeEntry(TimeEntry timeEntry, TogglApiKey apiKey, ILogger logger);
}