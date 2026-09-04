import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import { resolve } from 'path';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [plugin()],
  server: {
    port: 50535,
  },
  build: {
    rollupOptions: {
      input: {
        main: resolve(import.meta.dirname, 'index.html'),
        redirect: resolve(import.meta.dirname, 'redirect.html'),
      },
    },
  },
})
