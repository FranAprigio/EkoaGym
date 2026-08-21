namespace EkoaGym.Mobile.Services.Notifications;

public interface IRestTimerNotifier
{
    Task NotifyRestOverAsync(string exerciseName);
}
