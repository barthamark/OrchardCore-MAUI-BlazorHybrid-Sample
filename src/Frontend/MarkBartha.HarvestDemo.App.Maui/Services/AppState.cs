using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarkBartha.HarvestDemo.App.Maui.GraphQL;
using MarkBartha.HarvestDemo.Domain.Models;
using Microsoft.Extensions.Logging;

namespace MarkBartha.HarvestDemo.App.Maui.Services;

public class AppState
{
    private readonly IHarvestDemoClient _harvestDemoClient;
    private readonly ILogger<AppState> _logger;
    private readonly SemaphoreSlim _userProfileLock = new(1, 1);

    public AppState(IHarvestDemoClient harvestDemoClient, ILogger<AppState> logger)
    {
        _harvestDemoClient = harvestDemoClient;
        _logger = logger;
    }

    public UserProfile? UserProfile { get; private set; }
    public bool IsUserProfileLoading { get; private set; }
    public Exception? UserProfileError { get; private set; }

    public event EventHandler? OnChanged;

    public async Task EnsureUserProfileAsync(CancellationToken cancellationToken = default)
    {
        if (UserProfile is not null)
        {
            return;
        }

        await _userProfileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (UserProfile is not null)
            {
                return;
            }

            IsUserProfileLoading = true;
            UserProfileError = null;
            NotifyStateChanged();

            var result = await _harvestDemoClient.GetMe.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            if (result.Errors?.Count > 0)
            {
                var message = string.Join(", ", result.Errors.Select(error => error.Message));
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(message)
                        ? "Unknown GraphQL error while loading user profile."
                        : message);
            }

            var me = result.Data?.Me;
            if (me is null)
            {
                _logger.LogWarning("GraphQL 'me' query returned no user data.");
                return;
            }

            SetUserProfile(new UserProfile
            {
                UserId = me.UserId,
                UserName = me.UserName,
                Email = me.Email ?? string.Empty,
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            UserProfileError = ex;
            _logger.LogError(ex, "Failed to load the current user profile.");
        }
        finally
        {
            IsUserProfileLoading = false;
            _userProfileLock.Release();
            NotifyStateChanged();
        }
    }

    public void SetUserProfile(UserProfile profile)
    {
        UserProfile = profile;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChanged?.Invoke(this, EventArgs.Empty);
}
