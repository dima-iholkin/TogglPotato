﻿using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries.Models;

namespace TogglPotato.IntegrationTests;

public class TestOrganizeDailyTimeEntriesEndpoint
{
    [Fact]
    public async Task WorkflowShouldReturnSuccess()
    {
        string? togglApiKey = Environment.GetEnvironmentVariable("TogglPotato_TogglApiKey");
        if (togglApiKey is null)
        {
            throw new ArgumentNullException(
                nameof(togglApiKey), "Environment variable TogglPotato_TogglApiKey was not found."
            );
        }

        await using var application = new WebApplicationFactory<Program>();
        using HttpClient client = application.CreateClient();

        RequestBody requestBody = new RequestBody(
            togglApiKey, Date: new DateOnly(2024, 1, 31)
        );
        HttpResponseMessage result = await client.PostAsJsonAsync("/api/v1/organize_daily_time_entries", requestBody);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}