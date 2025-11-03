using OpenIddict.Abstractions;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.YesSql.Models;
using OrchardCore.Settings;

namespace MarkBartha.HarvestDemo.OrchardCore.Theme;

public class OpenIdApplicationMigrations : DataMigration
{
    private readonly IOpenIdApplicationManager _openIdApplicationManager;
    private readonly IOpenIdServerService _openIdServerService;
    private readonly ISiteService _siteService;
    private readonly IIdGenerator _idGenerator;

    public OpenIdApplicationMigrations(
        IOpenIdApplicationManager openIdApplicationManager,
        IOpenIdServerService openIdServerService,
        ISiteService siteService,
        IIdGenerator idGenerator)
    {
        _openIdApplicationManager = openIdApplicationManager;
        _openIdServerService = openIdServerService;
        _siteService = siteService;
        _idGenerator = idGenerator;
    }

    public async Task<int> CreateAsync()
    {
        await _openIdApplicationManager.CreateAsync(new OpenIdApplication
        {
            ApplicationId = _idGenerator.GenerateUniqueId(),
            ClientId = "harvestdemo.maui",
            DisplayName = "Harvest Demo - MAUI",
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            Permissions =
            [
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.EndSession,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Email,
            ],
            PostLogoutRedirectUris = ["harvestdemo://callback", "http://127.0.0.1:7605/callback/"],
            RedirectUris = ["harvestdemo://callback", "http://127.0.0.1:7605/callback/"],
            Requirements = [OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange],
            Type = OpenIddictConstants.ClientTypes.Public,
        });

        await _openIdApplicationManager.CreateAsync(new OpenIdApplication
        {
            ApplicationId = _idGenerator.GenerateUniqueId(),
            ClientId = "harvestdemo.web",
            DisplayName = "Harvest Demo - Web",
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            Permissions =
            [
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.EndSession,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Email,
            ],
            PostLogoutRedirectUris =
            [
                "https://localhost:7501",
                "https://localhost:7511"
            ],
            RedirectUris =
            [
                "https://localhost:7511/authentication/login-callback",
                "https://localhost:7501/app/authentication/login-callback",
            ],
            Requirements = [OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange],
            Type = OpenIddictConstants.ClientTypes.Public,
        });

        var openIdServerSettings = await _openIdServerService.GetSettingsAsync();
        openIdServerSettings.AccessTokenFormat = OpenIdServerSettings.TokenFormat.JsonWebToken;
        await _openIdServerService.UpdateSettingsAsync(openIdServerSettings);

        return 1;
    }
}
