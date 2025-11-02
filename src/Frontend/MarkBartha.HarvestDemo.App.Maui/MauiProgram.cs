using System.Net.Http.Headers;
using Duende.IdentityModel.OidcClient;
using MarkBartha.HarvestDemo.App.Maui.Services;
using MarkBartha.HarvestDemo.App.Maui.Services.Todos;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
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
            Scope = "openid offline_access email",
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

        builder.Services.AddSingleton<AppState>();
        builder.Services.AddSingleton<ITodoService, InMemoryTodoService>();
        builder.Services.AddHttpClient<TodoApiService>(client =>
        {
            client.BaseAddress = new Uri(AppConfig.ApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });
        builder.Services.AddScoped<TodoState>();
        
        builder.Services.AddHarvestDemoClient().ConfigureHttpClient(client =>
            client.BaseAddress = new Uri(AppConfig.ApiBaseUrl + "graphql"));

        return builder.Build();
    }
}
