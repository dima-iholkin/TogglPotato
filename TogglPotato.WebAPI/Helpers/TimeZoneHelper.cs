using System.Runtime.InteropServices;
using TimeZoneConverter;

namespace TogglPotato.WebAPI.Helpers;

public class TimeZoneHelper(ILogger<TimeZoneHelper> logger)
{
    public TimeZoneInfo FindTimeZoneFromToggl(string tzString)
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
}