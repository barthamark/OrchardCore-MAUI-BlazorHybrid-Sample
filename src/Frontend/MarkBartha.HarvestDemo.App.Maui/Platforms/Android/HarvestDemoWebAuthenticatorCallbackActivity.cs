using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Authentication;

namespace MarkBartha.HarvestDemo.App.Maui;

[Activity(Exported = true, NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
    DataScheme = "harvestdemo",
    DataHost = "callback")]
public class HarvestDemoWebAuthenticatorCallbackActivity : WebAuthenticatorCallbackActivity
{
}
