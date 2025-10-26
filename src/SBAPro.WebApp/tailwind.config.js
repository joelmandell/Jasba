/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,html,cshtml}",
    "./Pages/**/*.{razor,html,cshtml}",
  ],
  theme: {
    extend: {
      colors: {
        'primary': '#1b6ec2',
        'primary-dark': '#1861ac',
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
  ],
}
