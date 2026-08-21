using Microsoft.Maui.Devices;

namespace EkoaGym.Mobile.Services.Api;

// Base API URL per build configuration/platform. Debug targets local dev/staging, Release targets production.
public static class ApiEndpointOptions
{
#if DEBUG
    private const string LocalHttpsPort = "7025";

    // The Android emulator can't reach the host machine via "localhost" — it maps host loopback to 10.0.2.2.
    public static string BaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? $"https://10.0.2.2:{LocalHttpsPort}/"
            : $"https://localhost:{LocalHttpsPort}/";
#else
    // TODO: replace with the deployed production API URL before release.
    public static string BaseUrl => "https://api.ekoagym.com/";
#endif
}
