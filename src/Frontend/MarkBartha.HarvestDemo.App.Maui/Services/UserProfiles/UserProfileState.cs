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
    private UserProfile _profile;
    private bool _isDisposed;

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

    public UserProfile Profile => _profile ?? new UserProfile();
    public bool HasProfile => _profile != null;

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

    public Exception Error { get; private set; }

    public event Action StateChanged = delegate { };

    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return;
        }

        await ReloadAsync(cancellationToken);
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_isDisposed)
            {
                return;
            }

            IsLoading = true;
            Error = null;

            var profile = await _userProfileService.GetUserProfileAsync(cancellationToken);
            _profile = profile;
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
        if (_isDisposed)
        {
            return;
        }

        try
        {
            var authenticationState = await authenticationStateTask;
            var identity = authenticationState.User.Identity;
            if (identity == null || !identity.IsAuthenticated)
            {
                ClearProfile();
                return;
            }

            await ReloadAsync(CancellationToken.None);
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
        _profile = null;
        Error = null;
        _isInitialized = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => StateChanged();

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(UserProfileState));
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _authenticationStateProvider.AuthenticationStateChanged -= HandleAuthenticationStateChangedAsync;
        _isDisposed = true;
    }
}
