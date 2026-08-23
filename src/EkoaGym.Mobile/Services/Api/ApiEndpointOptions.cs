using Microsoft.Maui.Devices;

namespace EkoaGym.Mobile.Services.Api;

// Base API URL per build configuration/platform. Debug targets local dev/staging, Release targets production.
public static class ApiEndpointOptions
{
#if DEBUG
    private const string LocalHttpsPort = "7025";
    private const string AndroidEmulatorHost = "10.0.2.2";
    // LAN IP of the dev machine running the API — required for a physical phone on the same Wi-Fi.
    // Update this if your PC's IP changes (run `ipconfig` and look under your Wi-Fi adapter).
    private const string PhysicalDeviceLanHost = "192.168.100.5";

    // The Android emulator can't reach the host machine via "localhost" — it maps host loopback to 10.0.2.2.
    // A physical device instead needs the dev machine's actual LAN IP.
    public static string BaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? $"https://{(DeviceInfo.DeviceType == DeviceType.Virtual ? AndroidEmulatorHost : PhysicalDeviceLanHost)}:{LocalHttpsPort}/"
            : $"https://localhost:{LocalHttpsPort}/";

    // The ASP.NET Core dev cert isn't trusted by the emulator/device OS, so only for the local dev host,
    // accept it without validation. Never applies to the production BaseUrl above.
    public static HttpMessageHandler CreateDevHttpMessageHandler() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
    };
#else
    public static string BaseUrl => "https://ekoagym-api.victoriousglacier-a845754b.brazilsouth.azurecontainerapps.io/";
#endif
}
