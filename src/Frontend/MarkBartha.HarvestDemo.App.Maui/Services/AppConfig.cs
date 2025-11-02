namespace MarkBartha.HarvestDemo.App.Maui.Services;

public enum EnvironmentType
{
    Development,
    Production,
}

public static class AppConfig
{
#if DEBUG

    public static EnvironmentType Environment => EnvironmentType.Development;
    public static string BackendBaseUrl => "https://localhost:7501";
#else
    public static EnvironmentType Environment => EnvironmentType.Production;
    public static string BackendBaseUrl => "https://forespend.com";
#endif
    public static string ApiBaseUrl => BackendBaseUrl + "/api/";

    public static string CallbackUrl => DeviceInfo.Current.Platform == DevicePlatform.WinUI
        ? "http://127.0.0.1:7605/callback/"
        : "forespend://callback";

    public static bool IsDevelopment() => Environment == EnvironmentType.Development;
}
