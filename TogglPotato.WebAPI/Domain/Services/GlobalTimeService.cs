using System.Runtime.InteropServices;
using TimeZoneConverter;

namespace TogglPotato.WebAPI.Domain.Services;

public class GlobalTimeService(ILogger<GlobalTimeService> logger)
{
    public TimeZoneInfo FindTimeZoneFromTogglString(string tzString)
    {
        if (String.IsNullOrEmpty(tzString))
        {
            throw new ArgumentException("tzString is null or empty.", nameof(tzString));
        }

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string windowsTzName = TZConvert.IanaToWindows(tzString);
                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(windowsTzName);
                return tzInfo;
            }
            else
            {
                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzString);
                return tzInfo;
            }
        }
        catch (Exception ex) when (
            ex is InvalidTimeZoneException
            || ex is TimeZoneNotFoundException
        )
        {
            logger.LogError(
                "TimeZone with TimeZoneString {TimeZoneString} wasn't found on OS {OSDescription} " +
                    "with Exception {Exception}.",
                tzString, RuntimeInformation.OSDescription, ex
            );

            throw;
        }
    }

    public TimeSpan GetDailyTimeSpan(TimeZoneInfo tzInfo, DateOnly date)
    {
        (DateTime startDateUtc, DateTime endDateUtc) = this.GenerateUtcTimeRangeForDailyTimeEntries(tzInfo, date);

        TimeSpan dailyTimeSpan = endDateUtc.AddTicks(1) - startDateUtc;
        return dailyTimeSpan;
    }

    public (DateTime startDateUtc, DateTime endDateUtc) GenerateUtcTimeRangeForDailyTimeEntries(
        TimeZoneInfo tzInfo, DateOnly date
    )
    {
        TimeOnly zeroTime = TimeOnly.MinValue;

        DateTime startDate = new DateTime(date, zeroTime, DateTimeKind.Unspecified);
        DateTime _startDateUtc = TimeZoneInfo.ConvertTimeToUtc(startDate, tzInfo);

        DateTime endDate = new DateTime(date.AddDays(1), zeroTime, DateTimeKind.Unspecified);
        DateTime _endDateUtc = TimeZoneInfo.ConvertTimeToUtc(endDate, tzInfo).AddTicks(-1);

        return (_startDateUtc, _endDateUtc);
    }

    public string ToTogglApiString(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.ff'Z'");
    }
}