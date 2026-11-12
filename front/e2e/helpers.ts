import { expect, type Page } from '@playwright/test';

export const API = process.env.VITE_API_URL ?? 'http://127.0.0.1:5080';
export const SENHA = 'Senha@123';

/** E-mail único por execução: a API não aceita e-mail repetido nem voto/inscrição duplicados. */
export function emailUnico(prefixo: string) {
  return `${prefixo}.${Date.now()}.${Math.floor(Math.random() * 1e4)}@e2e.local`;
}

export async function entrar(page: Page, email: string, senha = SENHA) {
  await page.goto('/entrar');
  await page.getByLabel('E-mail').fill(email);
  await page.getByLabel('Senha', { exact: true }).fill(senha);
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).not.toHaveURL(/\/entrar/); // espera o login concluir antes de navegar
}

export async function sair(page: Page) {
  await page.evaluate(() => localStorage.removeItem('oportuniza.sessao'));
}

/** Preenche o endereço. Digita o CEP (dispara o ViaCEP) e garante os campos mesmo sem internet. */
export async function preencherEndereco(page: Page, bairro: string) {
  await page.getByLabel('CEP').fill('05767000');
  await page.waitForTimeout(800);
  const rua = page.getByLabel('Rua / logradouro');
  if (!(await rua.inputValue())) await rua.fill('Rua de Teste');
  await page.getByLabel('Bairro').fill(bairro);
  await page.getByLabel('Cidade').fill('São Paulo');
  await page.getByLabel('UF').fill('SP');
}

export async function preencherSenha(page: Page) {
  await page.getByLabel('Senha', { exact: true }).fill(SENHA);
  await page.getByLabel('Confirme a senha').fill(SENHA);
  await page.getByRole('checkbox', { name: /LGPD/ }).check();
}

export async function cadastrarCidadao(page: Page, nome: string, bairro = 'Capão Redondo') {
  const email = emailUnico('cidadao');
  await page.goto('/cadastro');
  await page.getByLabel('Nome completo').fill(nome);
  await page.getByLabel('E-mail').fill(email);
  await page.getByLabel('Data de nascimento').fill('2004-06-15');
  await page.getByRole('checkbox', { name: 'Tecnologia' }).check({ force: true });
  await preencherEndereco(page, bairro);
  await preencherSenha(page);
  await page.getByRole('button', { name: 'Criar minha conta' }).click();
  await expect(page).toHaveURL(/\/minha-area/);
  return email;
}

export async function cadastrarOrganizacao(page: Page, nome: string) {
  const email = emailUnico('org');
  await page.goto('/cadastro?perfil=organizacao');
  await page.getByLabel('Nome da organização').fill(nome);
  await page.getByLabel('E-mail').fill(email);
  await page.getByLabel('Descrição').fill('Organização criada pelo teste E2E.');
  await page.getByLabel('Site').fill('https://exemplo.org');
  await preencherEndereco(page, 'Capão Redondo');
  await preencherSenha(page);
  await page.getByRole('button', { name: 'Criar minha conta' }).click();
  await expect(page).toHaveURL(/\/organizacao$/);
  return email;
}
