namespace TogglPotato.WebAPI.Validators;

public static class InputDateValidator
{
    public static bool Validate(DateTime date)
    {
        if (date.TimeOfDay != new TimeSpan())
        {
            return false;
        }

        return true;
    }
}