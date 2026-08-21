using Microsoft.JSInterop;

namespace EkoaGym.Mobile.Services.Motion;

public class MotionService(IJSRuntime js) : IMotionService
{
    private bool? reducedMotionCache;

    public async Task<bool> PrefersReducedMotionAsync()
    {
        reducedMotionCache ??= await js.InvokeAsync<bool>("ekoagymMotion.prefersReducedMotion");
        return reducedMotionCache.Value;
    }

    public Task BurstConfettiAsync(params string[] colors) =>
        js.InvokeVoidAsync("ekoagymMotion.burstConfetti", (object)colors).AsTask();
}
