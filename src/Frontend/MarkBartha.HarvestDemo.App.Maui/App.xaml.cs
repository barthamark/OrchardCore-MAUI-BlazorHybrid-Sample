namespace MarkBartha.HarvestDemo.App.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "MarkBartha.HarvestDemo.App.Maui" };
    }
}