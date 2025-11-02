using MarkBartha.HarvestDemo.Domain.Models;

namespace MarkBartha.HarvestDemo.App.Maui.Services;

public class AppState
{
    public UserProfile UserProfile { get; private set; }

    public event EventHandler OnChanged;

    public void SetUserProfile(UserProfile profile)
    {
        UserProfile = profile;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChanged?.Invoke(this, EventArgs.Empty);
}
