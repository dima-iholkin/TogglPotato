using System.Net;

namespace TogglPotato.WebAPI.HttpClients.TogglApiErrors;

public class UnexpectedTogglApiError
{
    public UnexpectedTogglApiError(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
        Message = $"An unexpected error with status code {(int)StatusCode} returned from Toggl API.";
    }

    public string Message { get; init; }

    public HttpStatusCode StatusCode { get; init; }
}