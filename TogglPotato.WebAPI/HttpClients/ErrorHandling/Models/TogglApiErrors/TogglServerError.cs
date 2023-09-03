using System.Net;

namespace TogglPotato.WebAPI.HttpClients.ErrorHandling.Models.TogglApiErrors;

public class TogglServerError
{
    public TogglServerError(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
        Message = $"A {(int)StatusCode} error occured on Toggl API.";
    }

    public string Message { get; init; }

    public HttpStatusCode StatusCode { get; init; }
}