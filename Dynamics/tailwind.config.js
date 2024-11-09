/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './Pages/**/*.cshtml',
        './Views/**/*.cshtml',
        './Areas/**/*.cshtml',
    ],
    theme: {
        extend: {
            fontFamily: {
                'mona-sans': ['Mona-Sans', 'sans-serif'],
            },
        },
    },

    daisyui: {
        themes: [
            {
                mytheme: {
                    primary: "#1E429F",
                    // "primary-content": "#2F435A",
                    secondary: "#eab308",
                    "secondary-content": "#FFFFFF",
                    accent: "#D29191",
                    // "accent-content": "#150f00",
                    neutral: "#696E78",
                    // "neutral-content": "#000000",
                    "base-100": "#F5F5F5",
                    "base-200": "#D3D4D6",
                    "base-300": "#E2E7E9",
                    // "base-content": "#150f00",
                    info: "#0369a1",
                    "info-content": "#f5f5f5",
                    success: "#15803d",
                    "success-content": "#f5f5f5",
                    warning: "#FF8225",
                    "warning-content": "#FFFFFF",
                    error: "#B91C1C",
                    "error-content": "#f5f5f5",
                },
            },
        ],
    },
    plugins: [
        require('daisyui'),
        require('@tailwindcss/line-clamp'),
    ],
}

