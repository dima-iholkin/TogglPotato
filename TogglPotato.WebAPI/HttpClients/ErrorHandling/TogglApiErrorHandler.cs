using System.Net;
using OneOf;
using TogglPotato.WebAPI.HttpClients.ErrorHandling.Models;
using TogglPotato.WebAPI.HttpClients.ErrorHandling.Models.TogglApiErrors;

namespace TogglPotato.WebAPI.HttpClients.ErrorHandling;

public static class TogglApiErrorHandler
{
    public static async Task<TogglApiErrorResult> HandleHttpErrorsAsync(HttpResponseMessage response, ILogger logger)
    {
        OneOf<TogglApiKeyError, TogglServerError, TooManyRequestsError, UnexpectedTogglApiError> togglApiError =
            response.StatusCode switch
            {
                // 400 error codes:
                HttpStatusCode.Forbidden => new TogglApiKeyError(),
                HttpStatusCode.TooManyRequests => new TooManyRequestsError(),
                // 500 error codes:
                HttpStatusCode.BadGateway => new TogglServerError(HttpStatusCode.BadGateway),
                HttpStatusCode.InternalServerError => new TogglServerError(HttpStatusCode.InternalServerError),
                HttpStatusCode.ServiceUnavailable => new TogglServerError(HttpStatusCode.ServiceUnavailable),
                HttpStatusCode.GatewayTimeout => new TogglServerError(HttpStatusCode.GatewayTimeout),
                // other codes codes:
                _ => new UnexpectedTogglApiError(response.StatusCode)
            };

        TogglApiErrorResult errorResult = new TogglApiErrorResult(togglApiError);

        logger.LogWarning(
            "Toggl API UserProfile request returned error with StatusCode {StatusCode}.",
            response.StatusCode.ToString()
        );
        logger.LogWarning("Toggl API error message: {ErrorMessage}", await response.Content.ReadAsStringAsync());

        return errorResult;
    }

    public static IResult HandleTogglApiServiceErrors(TogglApiErrorResult errorResult)
    {
        IResult httpResult = errorResult.Error.Match<IResult>(
                (TogglApiKeyError togglApiError) => TypedResults.BadRequest(new { TogglApiKeyError.Message }),
                (TogglServerError togglServerError) => Results.Json(
                    new { togglServerError.Message }, statusCode: (int)HttpStatusCode.InternalServerError
                ),
                (TooManyRequestsError tooManyRequest) => Results.Json(
                    new { TooManyRequestsError.Message }, statusCode: (int)HttpStatusCode.TooManyRequests
                ),
                (UnexpectedTogglApiError unexpectedTogglApiError) => Results.Json(
                    new { unexpectedTogglApiError.Message }, statusCode: (int)HttpStatusCode.InternalServerError
                )
        );
        return httpResult;
    }
}