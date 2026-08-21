namespace EkoaGym.Mobile.Services.Motion;

// Thin wrapper over wwwroot/js/motion.js (M-9.3/M-9.4) — checks prefers-reduced-motion before any
// JS-driven celebration effect, since the CSS-only blanket override in app.tailwind.css can't reach code
// that decides in C# whether to run a JS effect at all (e.g. skipping the confetti burst entirely).
public interface IMotionService
{
    Task<bool> PrefersReducedMotionAsync();
    Task BurstConfettiAsync(params string[] colors);
}
