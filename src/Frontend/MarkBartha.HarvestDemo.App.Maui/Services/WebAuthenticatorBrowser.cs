using System.Diagnostics;
using Duende.IdentityModel.OidcClient.Browser;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace MarkBartha.HarvestDemo.App.Maui.Services;

public class WebAuthenticatorBrowser : IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            // This is for Android/iOS where WebAuthenticator is used later.
            // var result = await WebAuthenticator.Default.AuthenticateAsync(
            //     new Uri(options.StartUrl),
            //     new Uri(options.EndUrl));
            //
            // var url = new RequestUrl("forespend://callback")
            //     .Create(new Parameters(result.Properties));
            //
            // return new BrowserResult { Response = url, ResultType = BrowserResultType.Success, };

            return await WebAuthenticatorBrowser.InvokeWindowsBrowserAsync(options, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return new BrowserResult { ResultType = BrowserResultType.UserCancel };
        }
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
}
