using TogglPotato.WebAPI.Domain.Services;

namespace TogglPotato.WebAPI.StartupTests;

public class StartupTester
{
    public static void RunTests(WebApplication app)
    {
        using (IServiceScope scope = app.Services.CreateScope())
        {
            ILogger<StartupTester> logger = scope.ServiceProvider.GetRequiredService<ILogger<StartupTester>>();
            logger.LogInformation("Started the startup tests.");

            TestFindTimeZoneFromStringAbleToFindThem(scope);

            logger.LogInformation("Successfully completed the startup tests.");
        }
    }

    private static void TestFindTimeZoneFromStringAbleToFindThem(IServiceScope scope)
    {
        GlobalTimeService timeService = scope.ServiceProvider.GetRequiredService<GlobalTimeService>();
        timeService.FindTimeZoneFromTogglString("Europe/Kyiv");
    }
}