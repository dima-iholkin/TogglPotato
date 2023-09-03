using OneOf;
using TogglPotato.WebAPI.HttpClients.TogglApiErrors;

namespace TogglPotato.WebAPI.HttpClients.Models;

public class TogglApiErrorResult
{
    public TogglApiErrorResult(
        OneOf<TogglApiKeyError, TogglServerError, TooManyRequestsError, UnexpectedTogglApiError> error
    )
    {
        Error = error;
    }

    public OneOf<TogglApiKeyError, TogglServerError, TooManyRequestsError, UnexpectedTogglApiError> Error { get; init; }
}