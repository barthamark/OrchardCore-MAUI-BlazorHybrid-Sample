using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MarkBartha.HarvestDemo.App.Maui.Services;

internal static class HttpResponseMessageExtensions
{
    public static async Task EnsureSuccessAsync(
        this HttpResponseMessage response,
        Func<HttpResponseMessage, string, Exception> exceptionFactory,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var details = await response.Content.ReadAsStringAsync(cancellationToken);
        throw exceptionFactory(response, details);
    }
}
