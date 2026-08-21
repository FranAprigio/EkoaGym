// Plain global functions (not an ES module) since every call site just needs a couple of one-off
// interop calls, not a whole importable module. Dependency-free — no canvas-confetti package added,
// same "no heavy dependency for a small visual effect" philosophy as Components/Shared/LineChart.razor.
window.ekoagymMotion = (function () {
    function prefersReducedMotion() {
        return !!(window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches);
    }

    function burstConfetti(colors) {
        if (prefersReducedMotion()) {
            return;
        }

        var canvas = document.createElement("canvas");
        canvas.style.position = "fixed";
        canvas.style.inset = "0";
        canvas.style.zIndex = "9999";
        canvas.style.pointerEvents = "none";
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        document.body.appendChild(canvas);
        var ctx = canvas.getContext("2d");

        var particles = [];
        for (var i = 0; i < 120; i++) {
            particles.push({
                x: canvas.width / 2,
                y: canvas.height / 3,
                vx: (Math.random() - 0.5) * 12,
                vy: (Math.random() - 1.5) * 12,
                size: 4 + Math.random() * 4,
                color: colors[Math.floor(Math.random() * colors.length)],
                rotation: Math.random() * Math.PI * 2,
                spin: (Math.random() - 0.5) * 0.3,
            });
        }

        var start = performance.now();
        var durationMs = 1500;

        function frame(now) {
            var elapsed = now - start;
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            for (var j = 0; j < particles.length; j++) {
                var p = particles[j];
                p.vy += 0.35;
                p.x += p.vx;
                p.y += p.vy;
                p.rotation += p.spin;
                ctx.save();
                ctx.translate(p.x, p.y);
                ctx.rotate(p.rotation);
                ctx.fillStyle = p.color;
                ctx.fillRect(-p.size / 2, -p.size / 2, p.size, p.size);
                ctx.restore();
            }
            if (elapsed < durationMs) {
                requestAnimationFrame(frame);
            } else {
                canvas.remove();
            }
        }
        requestAnimationFrame(frame);
    }

    return { prefersReducedMotion: prefersReducedMotion, burstConfetti: burstConfetti };
})();
