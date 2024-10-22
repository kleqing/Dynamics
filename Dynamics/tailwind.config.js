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
                    secondary: "#FACC15",
                    // "secondary-content": "#150f00",
                    accent: "#D29191",
                    // "accent-content": "#150f00",
                    neutral: "#696E78",
                    // "neutral-content": "#000000",
                    "base-100": "#F5F5F5",
                    "base-200": "#D3D4D6",
                    "base-300": "#E2E7E9",
                    // "base-content": "#150f00",
                    info: "#1E429F",
                    "info-content": "#f5f5f5",
                    success: "#22c55e",
                    "success-content": "#f5f5f5",
                    warning: "#FF8225",
                    "warning-content": "#f5f5f5",
                    error: "#ED4050",
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

