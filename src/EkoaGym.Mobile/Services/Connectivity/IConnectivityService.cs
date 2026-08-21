namespace EkoaGym.Mobile.Services.Connectivity;

public interface IConnectivityService
{
    bool IsConnected { get; }

    event EventHandler<bool>? ConnectivityChanged;
}
