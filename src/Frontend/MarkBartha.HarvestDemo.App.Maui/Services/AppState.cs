using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MarkBartha.HarvestDemo.Domain.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace MarkBartha.HarvestDemo.App.Maui.Services;

public class AppState : IDisposable
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<AppState> _logger;
    private readonly SemaphoreSlim _userProfileLock = new(1, 1);

    public AppState(AuthenticationStateProvider authenticationStateProvider, ILogger<AppState> logger)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;

        _authenticationStateProvider.AuthenticationStateChanged += HandleAuthenticationStateChangedAsync;
    }

    public UserProfile? UserProfile { get; private set; }
    public bool IsUserProfileLoading { get; private set; }
    public Exception? UserProfileError { get; private set; }

    public event EventHandler? OnChanged;

    public async Task EnsureUserProfileAsync(CancellationToken cancellationToken = default)
    {
        if (UserProfile is not null || IsUserProfileLoading)
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

            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);
            UpdateUserProfile(authenticationState.User);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            UserProfileError = ex;
            _logger.LogError(ex, "Failed to load the current user profile from authentication state.");
        }
        finally
        {
            IsUserProfileLoading = false;
            _userProfileLock.Release();
            NotifyStateChanged();
        }
    }

    public void SetUserProfile(UserProfile? profile)
    {
        UserProfile = profile;
        NotifyStateChanged();
    }

    private async void HandleAuthenticationStateChangedAsync(Task<AuthenticationState> authenticationStateTask)
    {
        try
        {
            var authenticationState = await authenticationStateTask.ConfigureAwait(false);
            UpdateUserProfile(authenticationState.User);
        }
        catch (Exception ex)
        {
            UserProfileError = ex;
            _logger.LogError(ex, "Failed to update the user profile after an authentication change.");
            NotifyStateChanged();
        }
    }

    private void UpdateUserProfile(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            UserProfileError = null;
            SetUserProfile(null);
            return;
        }

        var userId = GetClaimValue(principal, "sub") ?? string.Empty;
        var userName = GetClaimValue(principal, ClaimTypes.Name) ?? principal.Identity?.Name ?? string.Empty;
        var email = GetClaimValue(principal, "email") ?? string.Empty;

        UserProfileError = null;
        SetUserProfile(new UserProfile
        {
            UserId = userId,
            UserName = userName,
            Email = email,
        });
    }

    private static string? GetClaimValue(ClaimsPrincipal principal, string claimType) =>
        principal.FindFirst(claimType)?.Value;

    private void NotifyStateChanged() => OnChanged?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _authenticationStateProvider.AuthenticationStateChanged -= HandleAuthenticationStateChangedAsync;
        _userProfileLock.Dispose();
    }
}
