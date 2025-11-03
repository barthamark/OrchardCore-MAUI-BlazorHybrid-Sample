using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarkBartha.HarvestDemo.App.Maui.Services;

public sealed class LoopbackHttpListener : IDisposable
{
    private readonly string _path;
    private HttpListener _listener;
    private CancellationTokenSource _cts;

    private int Port { get; }
    public string RedirectUri => $"http://127.0.0.1:{Port}{(_path != null ? $"/{_path}/" : "/")}";

    public LoopbackHttpListener(int? port = null, string path = null)
    {
        _path = path;
        Port = port ?? GetRandomUnusedPort();

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
        if (!string.IsNullOrEmpty(_path))
        {
            _listener.Prefixes.Add($"http://127.0.0.1:{Port}/{_path}/");
        }

        _listener.Start();
    }

    private static int GetRandomUnusedPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);

        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }

    public async Task<string> WaitForCallbackAsync(int timeoutSeconds = 60)
    {
        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            var ctx = await _listener.GetContextAsync();
            var response = ctx.Response;
            response.StatusCode = 200; // OK
            response.ContentType = "text/html";
            var buffer = Encoding.UTF8.GetBytes("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"><titl" +
                "e>Done</title></head><body style=\"margin:0;background:#f5f3ff;color:#403099;font-family:Inter,syste" +
                "m-ui,-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;display:fl" +
                "ex;align-items:center;justify-content:center;height:100vh;font-size:1.25rem;font-weight:500;text-ali" +
                "gn:center;\">You can close this tab now.</body></html>\n");
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, _cts.Token);
            response.OutputStream.Close();

            return ctx.Request.Url?.ToString();
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("Browser callback timed out.");
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _listener.Stop();
        _listener.Close();
        _listener = null;
    }
}
