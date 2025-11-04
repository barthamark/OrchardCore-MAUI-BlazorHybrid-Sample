using System.Threading;
using System.Threading.Tasks;
using MarkBartha.HarvestDemo.Domain.Models;

namespace MarkBartha.HarvestDemo.App.Maui.Services.UserProfiles;

/// <summary>
/// Defines the contract for retrieving user profile information from the Orchard backend.
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Retrieves the profile for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The user profile provided by the backend.</returns>
    Task<UserProfile> GetUserProfileAsync(CancellationToken cancellationToken = default);
}
