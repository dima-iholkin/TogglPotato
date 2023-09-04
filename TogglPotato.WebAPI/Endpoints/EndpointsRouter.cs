using Microsoft.AspNetCore.Mvc;
using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries;
using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries.Models;

namespace TogglPotato.WebAPI.Endpoints;

public static class EndpointsRouter
{
    public static void Map(WebApplication app)
    {
        app.MapPost(
            "/api/organize_daily_time_entries",
            ([FromBody] RequestBody requestBody, [FromServices] OrganizeDailyTimeEntriesEndpoint endpoint) =>
                endpoint.Handler(requestBody)
        );
    }
}