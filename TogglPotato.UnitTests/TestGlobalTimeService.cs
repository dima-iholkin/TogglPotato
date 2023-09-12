using Microsoft.Extensions.Logging;
using Moq;
using TogglPotato.WebAPI.Domain.Services;

namespace TogglPotato.UnitTests;

public class TestGlobalTimeService()
{
    [Theory]
    [InlineData(2023, 3, 26, 23)]
    [InlineData(2022, 10, 30, 25)]
    [InlineData(2023, 9, 5, 24)]
    public void GetDailyTimeSpan_ShouldReturnCorrectTotalDailyTimeForDaylightSavingsDaysToo(
        int year, int month, int day, int expectedHours
    )
    {
        var loggerMock = Mock.Of<ILogger<GlobalTimeService>>();
        GlobalTimeService timeService = new GlobalTimeService(loggerMock);

        TimeZoneInfo tzInfo = timeService.FindTimeZoneFromTogglString("Europe/Kyiv");
        DateOnly date = new DateOnly(year, month, day);

        TimeSpan dailyTimeSpan = timeService.GetDailyTimeSpan(tzInfo, date);

        TimeSpan expectedTimeSpan = new TimeSpan(expectedHours, minutes: 0, seconds: 0);
        Assert.Equal(expectedTimeSpan, dailyTimeSpan);
    }

    [Theory]
    [InlineData("Europe/Kyiv")]
    public void FindTimeZoneFromTogglString_HappyPathShouldWork(string tzString)
    {
        var loggerMock = Mock.Of<ILogger<GlobalTimeService>>();
        GlobalTimeService timeService = new GlobalTimeService(loggerMock);

        TimeZoneInfo tzInfo = timeService.FindTimeZoneFromTogglString(tzString);

        Assert.NotNull(tzInfo);
    }
}