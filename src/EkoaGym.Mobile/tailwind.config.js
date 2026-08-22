/** @type {import('tailwindcss').Config} */
// M-9.2: one easing curve, reused for both discrete transitions (ease-app) and keyframe animations
// below, so all motion in the app stays visually consistent if the curve is ever tuned.
const appEasing = "cubic-bezier(0.4, 0, 0.2, 1)";

module.exports = {
  darkMode: "class",
  content: [
    "./Components/**/*.razor",
    "./wwwroot/index.html",
  ],
  theme: {
    extend: {
      colors: {
        // FitTrack design system — semantic tokens only, never hardcode hex in components.
        primary: "rgb(var(--color-primary) / <alpha-value>)",
        secondary: "rgb(var(--color-secondary) / <alpha-value>)",
        accent: "rgb(var(--color-accent) / <alpha-value>)",
        info: "rgb(var(--color-info) / <alpha-value>)",
        danger: "rgb(var(--color-danger) / <alpha-value>)",
        background: "rgb(var(--color-background) / <alpha-value>)",
        surface: "rgb(var(--color-surface) / <alpha-value>)",
        "text-primary": "rgb(var(--color-text-primary) / <alpha-value>)",
        "text-secondary": "rgb(var(--color-text-secondary) / <alpha-value>)",
        gold: "rgb(var(--color-gold) / <alpha-value>)",
        silver: "rgb(var(--color-silver) / <alpha-value>)",
        bronze: "rgb(var(--color-bronze) / <alpha-value>)",
        pending: "rgb(var(--color-pending) / <alpha-value>)",
      },
      borderColor: {
        DEFAULT: "rgb(var(--color-border) / <alpha-value>)",
      },
      transitionDuration: {
        transition: "250ms",
      },
      transitionTimingFunction: {
        app: appEasing,
      },
      keyframes: {
        fadeInUp: {
          "0%": { opacity: "0", transform: "translateY(8px)" },
          "100%": { opacity: "1", transform: "translateY(0)" },
        },
        slideInRight: {
          "0%": { opacity: "0", transform: "translateX(24px)" },
          "100%": { opacity: "1", transform: "translateX(0)" },
        },
        slideInLeft: {
          "0%": { opacity: "0", transform: "translateX(-24px)" },
          "100%": { opacity: "1", transform: "translateX(0)" },
        },
        slideUp: {
          "0%": { transform: "translateY(100%)" },
          "100%": { transform: "translateY(0)" },
        },
        backdropFade: {
          "0%": { opacity: "0" },
          "100%": { opacity: "1" },
        },
        popIn: {
          "0%": { opacity: "0", transform: "scale(0)" },
          "70%": { opacity: "1", transform: "scale(1.15)" },
          "100%": { opacity: "1", transform: "scale(1)" },
        },
        successPulse: {
          "0%": { boxShadow: "0 0 0 0 rgb(var(--color-accent) / 0.6)" },
          "100%": { boxShadow: "0 0 0 12px rgb(var(--color-accent) / 0)" },
        },
      },
      animation: {
        "fade-in-up": `fadeInUp 250ms ${appEasing} both`,
        "slide-in-right": `slideInRight 250ms ${appEasing} both`,
        "slide-in-left": `slideInLeft 250ms ${appEasing} both`,
        "modal-slide-up": `slideUp 250ms ${appEasing} both`,
        "backdrop-fade": `backdropFade 250ms ${appEasing} both`,
        "pr-pop": `popIn 400ms ${appEasing} both`,
        "success-pulse": `successPulse 500ms ${appEasing}`,
      },
    },
  },
  plugins: [],
};

