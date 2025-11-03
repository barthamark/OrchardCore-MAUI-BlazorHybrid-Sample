using System.Threading;
using System.Threading.Tasks;
using MarkBartha.HarvestDemo.Domain.Models;

namespace MarkBartha.HarvestDemo.App.Maui.Services.UserProfiles;

public interface IUserProfileService
{
    Task<UserProfile> GetUserProfileAsync(CancellationToken cancellationToken = default);
}
