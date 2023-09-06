using System.Net.Http.Headers;
using System.Text;
using TogglPotato.WebAPI.Models;

namespace TogglPotato.WebAPI.HttpClients.Helpers;

public static class HttpRequestHeadersExtensions
{
    public static void AddApiKeyAuthorization(this HttpRequestHeaders headers, TogglApiKey apiKey)
    {
        string basicToken = Convert.ToBase64String(
                ASCIIEncoding.ASCII.GetBytes($"{apiKey.Value}:api_token")
            );

        headers.Authorization = new AuthenticationHeaderValue("Basic", basicToken);
    }
}