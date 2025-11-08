using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient.Browser;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace MarkBartha.HarvestDemo.App.Maui.Services;

public class WebAuthenticatorBrowser : IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            if (IsMobilePlatform())
            {
                return await InvokeMobileAuthenticatorAsync(options);
            }

            return await WebAuthenticatorBrowser.InvokeWindowsBrowserAsync(options, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return new BrowserResult { ResultType = BrowserResultType.UserCancel };
        }
    }

    private static async Task<BrowserResult> InvokeMobileAuthenticatorAsync(BrowserOptions options)
    {
        var callbackUri = !string.IsNullOrWhiteSpace(options.EndUrl)
            ? new Uri(options.EndUrl)
            : new Uri(AppConfig.CallbackUrl);

        var result = await WebAuthenticator.Default.AuthenticateAsync(
            new WebAuthenticatorOptions
            {
                Url = new Uri(options.StartUrl),
                CallbackUrl = callbackUri
            });

        var response = new RequestUrl(callbackUri.ToString())
            .Create(new Parameters(result.Properties));

        return new BrowserResult { Response = response, ResultType = BrowserResultType.Success };
    }

    private static async Task<BrowserResult> InvokeWindowsBrowserAsync(BrowserOptions options, CancellationToken cancellationToken)
    {
        using var listener = new LoopbackHttpListener(7605, "callback");

        var startUrlWithDynamicRedirect = string.IsNullOrEmpty(options.EndUrl)
            ? options.StartUrl
            : options.StartUrl.Replace(
                Uri.EscapeDataString(options.EndUrl),
                Uri.EscapeDataString(listener.RedirectUri)
            );

        OpenBrowser(startUrlWithDynamicRedirect);

        try
        {
            var result = await listener.WaitForCallbackAsync((int)options.Timeout.TotalSeconds);

            if (string.IsNullOrWhiteSpace(result))
            {
                return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Empty response from browser callback." };
            }

            var uri = new Uri(result);
            var queryString = uri.Query;

            return new BrowserResult { Response = queryString, ResultType = BrowserResultType.Success };
        }
        catch (TaskCanceledException)
        {
            return new BrowserResult { ResultType = BrowserResultType.UserCancel };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Windows Browser Authentication Error: {ex.Message}");
            return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.ToString() };
        }
    }

    private static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open browser: {ex.Message}");

            throw;
        }
    }

    private static bool IsMobilePlatform()
    {
        var platform = DeviceInfo.Current.Platform;
        return platform == DevicePlatform.Android || platform == DevicePlatform.iOS;
    }
}
