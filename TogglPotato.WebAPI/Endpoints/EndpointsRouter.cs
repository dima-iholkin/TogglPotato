using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries;
using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries.Models;
using TogglPotato.WebAPI.Infrastructure.Swagger;

namespace TogglPotato.WebAPI.Endpoints;

public static class EndpointsRouter
{
    public static void Map(RouteGroupBuilder apiVersion)
    {
        apiVersion.MapPost("/organize_daily_time_entries", OrganizeDailyTimeEntriesHandler);
    }

    /// <summary>
    /// Reorganizes the Time Entries on the provided date.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/organize_daily_time_entries
    ///     {
    ///        "togglApiKey": "8888aaaaa4444bbbbbbb999999111111",
    ///        "date": "2023-08-30",
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns the organized time entries.</response>
    /// <response code="400">If the request isn't correct.</response>
    [SwaggerRequestExample(typeof(ExampleRequestBody), typeof(ExampleRequestProvider))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    private static Task<IResult> OrganizeDailyTimeEntriesHandler(
            [FromBody] RequestBody requestBody,
            [FromServices] OrganizeDailyTimeEntriesEndpoint endpoint,
            CancellationToken cancellationToken
        )
    {
        return endpoint.Handler(requestBody, cancellationToken);
    }
}