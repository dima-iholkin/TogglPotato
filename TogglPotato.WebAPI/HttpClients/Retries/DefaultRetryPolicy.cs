using Polly;
using Polly.Extensions.Http;

namespace TogglPotato.WebAPI.HttpClients.Retries;

public static class DefaultRetryPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(
            6,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
        );
    }
}