using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.HttpClients;

public interface ITogglHttpService
{
    public ValueTask<UserProfile> GetUserProfile(string apiKey, ILogger logger);

    public ValueTask<List<TimeEntry>> GetDailyTimeEntries(DateTime startTime, DateTime endTime, string apiKey, ILogger logger);

    public ValueTask<TimeEntry> UpdateTimeEntry(TimeEntry timeEntry, string apiKey, ILogger logger);
}