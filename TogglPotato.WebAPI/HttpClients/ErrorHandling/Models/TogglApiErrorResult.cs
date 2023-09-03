using OneOf;
using TogglPotato.WebAPI.HttpClients.ErrorHandling.Models.TogglApiErrors;

namespace TogglPotato.WebAPI.HttpClients.ErrorHandling.Models;

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