using System;
using System.Net.Http.Headers;
using Duende.IdentityModel.OidcClient;
using MarkBartha.HarvestDemo.App.Maui.Services;
using MarkBartha.HarvestDemo.App.Maui.Services.Todos;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace MarkBartha.HarvestDemo.App.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        
        builder.Services.AddSingleton<IBrowser, WebAuthenticatorBrowser>();
        builder.Services.AddSingleton(services => new OidcClient(new OidcClientOptions
        {
            Authority = AppConfig.BackendBaseUrl,
            Scope = "openid offline_access email profile",
            ClientId = "harvestdemo.maui",
            RedirectUri = AppConfig.CallbackUrl,
            PostLogoutRedirectUri = AppConfig.CallbackUrl,
            Browser = services.GetRequiredService<IBrowser>(),
            Policy = new Policy { RequireIdentityTokenSignature = false },
        }));
        
        builder.Services.AddSingleton<AuthenticatedUser>();
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<ExternalAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(services =>
            services.GetRequiredService<ExternalAuthenticationStateProvider>());

        builder.Services.AddScoped<AuthenticatedHttpClient>();
        builder.Services.AddHttpClient<AuthenticatedHttpClient>((_, client) =>
        {
            client.BaseAddress = new Uri(AppConfig.ApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });

        builder.Services.AddScoped<AppState>();
        builder.Services.AddScoped<ITodoService, TodoApiService>();
        builder.Services.AddScoped<TodoState>();

        return builder.Build();
    }
}
