import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const defaultApiProxyTarget =
  process.env.CHOKIDAR_USEPOLLING === 'true'
    ? 'http://api:5000'
    : 'http://localhost:5000'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      '/api': {
        target: process.env.API_PROXY_TARGET ?? defaultApiProxyTarget,
        changeOrigin: true,
      },
    },
  },
})
