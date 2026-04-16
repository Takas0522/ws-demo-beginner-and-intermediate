import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    host: '0.0.0.0',
    port: 3001,
    hmr: {
      clientPort: 3001,
    },
    proxy: {
      '/api/auth': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
      '/api/users': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
      '/api': {
        target: 'http://localhost:5001',
        changeOrigin: true,
      },
    },
  },
})
