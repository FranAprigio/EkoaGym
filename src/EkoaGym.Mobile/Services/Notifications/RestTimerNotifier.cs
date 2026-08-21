using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace EkoaGym.Mobile.Services.Notifications;

// Swallow failures (permission denied, platform quirks, etc.) instead of crashing the rest timer flow —
// the on-screen countdown already covers the core UX regardless of whether the OS notification fires.
public class RestTimerNotifier : IRestTimerNotifier
{
    public async Task NotifyRestOverAsync(string exerciseName)
    {
        try
        {
            var notification = new NotificationRequest
            {
                NotificationId = 1001,
                Title = "Rest complete",
                Description = $"Time for your next set of {exerciseName}!",
            };

            await LocalNotificationCenter.Current.Show(notification);
        }
        catch
        {
            // Unsupported platform or notification permission denied — the on-screen timer already covers this.
        }
    }
}
