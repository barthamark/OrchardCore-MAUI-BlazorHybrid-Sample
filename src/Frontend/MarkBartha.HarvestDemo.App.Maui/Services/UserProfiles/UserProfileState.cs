#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using MarkBartha.HarvestDemo.App.Maui.Exceptions;
using MarkBartha.HarvestDemo.Domain.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace MarkBartha.HarvestDemo.App.Maui.Services.UserProfiles;

public sealed class UserProfileState : IDisposable
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserProfileState> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _isInitialized;
    private bool _isLoading;

    public UserProfileState(
        AuthenticationStateProvider authenticationStateProvider,
        IUserProfileService userProfileService,
        ILogger<UserProfileState> logger)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userProfileService = userProfileService;
        _logger = logger;

        _authenticationStateProvider.AuthenticationStateChanged += HandleAuthenticationStateChangedAsync;
    }

    public UserProfile? Profile { get; private set; }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                NotifyStateChanged();
            }
        }
    }

    public Exception? Error { get; private set; }

    public event Action? StateChanged;

    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return;
        }

        await ReloadAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            IsLoading = true;
            Error = null;

            var profile = await _userProfileService.GetUserProfileAsync(cancellationToken).ConfigureAwait(false);
            Profile = profile;
            _isInitialized = true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (UserProfileServiceException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("User profile request returned 401 - clearing cached profile.");
            ClearProfile();
        }
        catch (Exception ex)
        {
            Error = ex;
            _logger.LogError(ex, "Failed to load the current user profile.");
        }
        finally
        {
            IsLoading = false;
            _gate.Release();
            NotifyStateChanged();
        }
    }

    private async void HandleAuthenticationStateChangedAsync(Task<AuthenticationState> authenticationStateTask)
    {
        try
        {
            var authenticationState = await authenticationStateTask.ConfigureAwait(false);
            if (authenticationState.User.Identity?.IsAuthenticated != true)
            {
                ClearProfile();
                return;
            }

            await ReloadAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Ignore expected cancellation.
        }
        catch (Exception ex)
        {
            Error = ex;
            _logger.LogError(ex, "Failed to refresh the user profile after an authentication change.");
            NotifyStateChanged();
        }
    }

    private void ClearProfile()
    {
        Profile = null;
        Error = null;
        _isInitialized = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    public void Dispose()
    {
        _authenticationStateProvider.AuthenticationStateChanged -= HandleAuthenticationStateChangedAsync;
        _gate.Dispose();
    }
}
