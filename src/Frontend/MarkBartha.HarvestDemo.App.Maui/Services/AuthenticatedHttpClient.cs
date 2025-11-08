using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MarkBartha.HarvestDemo.App.Maui.Services;

public class AuthenticatedHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ExternalAuthenticationStateProvider _authStateProvider;

    public AuthenticatedHttpClient(HttpClient httpClient, ExternalAuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
    }

    public async Task<HttpClient> GetClientAsync()
    {
        var token = await _authStateProvider.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            var authorization = _httpClient.DefaultRequestHeaders.Authorization;
            if (authorization == null || authorization.Parameter != token)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return _httpClient;
    }
}
