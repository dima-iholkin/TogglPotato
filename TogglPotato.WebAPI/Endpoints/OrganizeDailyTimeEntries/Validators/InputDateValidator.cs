namespace TogglPotato.WebAPI.Validators;

public static class InputDateValidator
{
    public static bool ValidateInputDateIsWithinLast3Months(DateOnly date)
    {
        DateTime dateTime3MonthsAgo = DateTime.UtcNow.AddMonths(-3);
        DateOnly date3MonthsAgo = DateOnly.FromDateTime(dateTime3MonthsAgo);

        // If the input date is older than 3 months ago, the Toggl GET Time Entries API wouldn't work.
        if (date < date3MonthsAgo)
        {
            return false;
        }

        return true;
    }
}