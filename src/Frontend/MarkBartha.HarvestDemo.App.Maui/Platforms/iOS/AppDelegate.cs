using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace MarkBartha.HarvestDemo.App.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}