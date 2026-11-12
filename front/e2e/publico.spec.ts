import { expect, test } from '@playwright/test';

test.describe('visitante', () => {
  test('home carrega indicadores ao vivo da API e os ODS', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('heading', { level: 1 })).toContainText('Qualificação gratuita');
    await expect(page.getByText('vagas gratuitas ofertadas')).toBeVisible();
    await expect(page.getByText('Dados ao vivo da API')).toBeVisible();
    await expect(page.getByRole('heading', { name: /4 Objetivos de Desenvolvimento Sustentável/ })).toBeVisible();
  });

  test('busca oportunidades com filtro e abre o detalhe', async ({ page }) => {
    await page.goto('/oportunidades');
    await expect(page.getByText(/oportunidades? encontradas?/)).toBeVisible();
    const primeiro = page.locator('article h3 a').first();
    await expect(primeiro).toBeVisible();

    const filtros = page.getByRole('button', { name: /Filtros/ });
    if (await filtros.isVisible()) await filtros.click(); // botão só aparece no celular
    await page.getByLabel('Área').selectOption('Tecnologia');
    await expect(page).toHaveURL(/area=Tecnologia/);
    await expect(page.locator('article').first()).toContainText('Tecnologia');

    await page.locator('article h3 a').first().click();
    await expect(page.getByRole('link', { name: 'Entrar para se inscrever' })).toBeVisible();
  });

  test('painel de impacto mostra gráficos e tabela acessível', async ({ page }) => {
    await page.goto('/impacto');
    await expect(page.getByText('Taxa de conclusão', { exact: true })).toBeVisible();
    const painel = page.getByRole('region', { name: 'Inscrições por bairro' });
    await expect(painel.locator('svg.recharts-surface')).toBeVisible();
    await painel.getByRole('button', { name: 'Tabela' }).click();
    await expect(painel.getByRole('table')).toBeVisible();
  });

  test('modo escuro persiste ao recarregar', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('button', { name: 'Ativar modo escuro' }).click();
    await expect(page.locator('html')).toHaveClass(/dark/);
    await page.reload();
    await expect(page.locator('html')).toHaveClass(/dark/);
  });

  test('área protegida manda para o login', async ({ page }) => {
    await page.goto('/minha-area');
    await expect(page).toHaveURL(/\/entrar/);
  });

  test('login com senha errada mostra mensagem genérica', async ({ page }) => {
    await page.goto('/entrar');
    await page.getByLabel('E-mail').fill('nao.existe@e2e.local');
    await page.getByLabel('Senha', { exact: true }).fill('SenhaErrada1');
    await page.getByRole('button', { name: 'Entrar', exact: true }).click();
    await expect(page.getByRole('alert')).toHaveText('E-mail ou senha incorretos.');
  });
});
