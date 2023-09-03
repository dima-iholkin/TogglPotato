using OneOf;
using OneOf.Types;
using TogglPotato.WebAPI.ValidationErrors;

namespace TogglPotato.WebAPI.Models;

public class TogglApiKey
{
    // Private constructor:

    private TogglApiKey(string togglApiKey)
    {
        Value = togglApiKey;
    }

    // Public factory:

    public static OneOf<TogglApiKey, TogglApiKeyValidationError> ConvertFrom(string togglApiKey)
    {
        if (Validate(togglApiKey) == false)
        {
            return new TogglApiKeyValidationError();
        }

        return new TogglApiKey(togglApiKey);
    }

    // Properties:

    public string Value { get; init; }

    // Validation:

    public static bool Validate(string togglApiKey)
    {
        if (String.IsNullOrEmpty(togglApiKey))
        {
            return false;
        }

        if (togglApiKey.Length < 5)
        {
            return false;
        }

        return true;
    }
}