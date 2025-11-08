using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace MarkBartha.HarvestDemo.App.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow([AllowNull] IActivationState activationState)
    {
        return new Window(new MainPage()) { Title = "Orchard Harvest 2025 Demo App" };
    }
}
