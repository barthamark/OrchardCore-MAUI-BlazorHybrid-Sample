using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MarkBartha.HarvestDemo.App.Maui.Exceptions;
using MarkBartha.HarvestDemo.Domain.Models;
using static MarkBartha.HarvestDemo.App.Maui.Constants.JsonOptions;

namespace MarkBartha.HarvestDemo.App.Maui.Services.UserProfiles;

public class UserProfileService : IUserProfileService
{
    private readonly AuthenticatedHttpClient _authenticatedHttpClient;

    public UserProfileService(AuthenticatedHttpClient authenticatedHttpClient)
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
            return profile ?? throw new UserProfileServiceException("The server returned an empty user profile payload.");
        }

        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new UserProfileServiceException($"Failed to load the user profile. Server returned {(int)response.StatusCode}: {message}")
        {
            StatusCode = response.StatusCode,
        };
    }
}
