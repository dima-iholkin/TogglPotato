namespace TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries.Models;

public record RequestBody(string TogglApiKey, DateOnly Date);