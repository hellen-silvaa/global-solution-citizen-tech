import { defineConfig, devices } from '@playwright/test';

// E2E contra a API REAL (http://127.0.0.1:5080 com MySQL). Suba a API antes:
//   ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Oportuniza.Api
// O front é iniciado aqui automaticamente (Vite na porta 5173).
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  workers: 1,
  retries: 0,
  timeout: 45_000,
  expect: { timeout: 10_000 },
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: 'http://localhost:5173',
    locale: 'pt-BR',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'desktop', use: { ...devices['Desktop Chrome'] } },
    { name: 'celular', use: { ...devices['Pixel 7'] }, testMatch: /publico\.spec/ },
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: true,
    timeout: 60_000,
  },
});
