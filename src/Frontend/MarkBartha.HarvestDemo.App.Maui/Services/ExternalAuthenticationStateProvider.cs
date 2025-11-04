using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityModel.OidcClient;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Maui.Storage;

namespace MarkBartha.HarvestDemo.App.Maui.Services;

public class ExternalAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly OidcClient _oidcClient;
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticatedUser _authenticatedUser;

    public ExternalAuthenticationStateProvider(OidcClient oidcClient, NavigationManager navigationManager)
    {
        _oidcClient = oidcClient;
        _navigationManager = navigationManager;
        _authenticatedUser = new AuthenticatedUser { Principal = new ClaimsPrincipal(new ClaimsIdentity()) };
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(_authenticatedUser.Principal));

    public async Task LoginAsync()
    {
        var result = await _oidcClient.LoginAsync();

        if (result.IsError)
        {
            throw new Exception(result.Error);
        }

        _authenticatedUser.Principal = result.User;

        // Store tokens
        await SecureStorage.SetAsync("access_token", result.AccessToken);
        await SecureStorage.SetAsync("refresh_token", result.RefreshToken);
        await SecureStorage.SetAsync("id_token", result.IdentityToken);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        _navigationManager.NavigateTo("/", forceLoad: true);
    }

    public async Task LogoutAsync()
    {
        await _oidcClient.LogoutAsync();

        // Clear stored tokens
        SecureStorage.Remove("access_token");
        SecureStorage.Remove("refresh_token");
        SecureStorage.Remove("id_token");

        _authenticatedUser.Principal = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        _navigationManager.NavigateTo("/", forceLoad: true);
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var token = await SecureStorage.GetAsync("access_token");
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        var refreshToken = await SecureStorage.GetAsync("refresh_token");
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var result = await _oidcClient.RefreshTokenAsync(refreshToken);
            if (!result.IsError)
            {
                await SecureStorage.SetAsync("access_token", result.AccessToken);
                await SecureStorage.SetAsync("refresh_token", result.RefreshToken);
                return result.AccessToken;
            }
        }

        return token;
    }

    public async Task<bool> TrySilentLoginAsync()
    {
        var accessToken = await SecureStorage.GetAsync("access_token");
        var refreshToken = await SecureStorage.GetAsync("refresh_token");
        var idToken = await SecureStorage.GetAsync("id_token");

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(idToken))
            return false;

        // Optional: refresh the token if you want to validate freshness
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var refreshResult = await _oidcClient.RefreshTokenAsync(refreshToken);
            if (!refreshResult.IsError)
            {
                accessToken = refreshResult.AccessToken;
                await SecureStorage.SetAsync("access_token", accessToken);
                await SecureStorage.SetAsync("refresh_token", refreshResult.RefreshToken);
                await SecureStorage.SetAsync("id_token", refreshResult.IdentityToken);

                var userInfo = await _oidcClient.GetUserInfoAsync(accessToken);

                var identity = new ClaimsIdentity(userInfo.Claims, "oidc");
                _authenticatedUser.Principal = new ClaimsPrincipal(identity);

                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                return true;
            }
        }

        return false;
    }
}

public class AuthenticatedUser
{
    public ClaimsPrincipal Principal { get; set; } = new();
}
