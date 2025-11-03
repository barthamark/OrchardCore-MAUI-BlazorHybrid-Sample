using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MarkBartha.HarvestDemo.Domain.Models;

namespace MarkBartha.HarvestDemo.App.Maui.Services.UserProfiles;

public class UserProfileApiService : IUserProfileService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly AuthenticatedHttpClient _authenticatedHttpClient;

    public UserProfileApiService(AuthenticatedHttpClient authenticatedHttpClient)
    {
        _authenticatedHttpClient = authenticatedHttpClient;
    }

    public async Task<UserProfile> GetUserProfileAsync(CancellationToken cancellationToken = default)
    {
        var client = await _authenticatedHttpClient.GetClientAsync();
        using var response = await client.GetAsync("user-profile", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var profile = await response.Content.ReadFromJsonAsync<UserProfile>(SerializerOptions, cancellationToken);
            if (profile is null)
            {
                throw new UserProfileServiceException("The server returned an empty user profile payload.");
            }

            return profile;
        }

        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new UserProfileServiceException($"Failed to load the user profile. Server returned {(int)response.StatusCode}: {message}")
        {
            StatusCode = response.StatusCode,
        };
    }
}
