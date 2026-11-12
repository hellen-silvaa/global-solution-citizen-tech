import { expect, test } from '@playwright/test';
import { cadastrarCidadao, cadastrarOrganizacao, entrar, sair } from './helpers';

// Fluxo de ponta a ponta contra a API real:
// organização publica → cidadão recebe recomendação e se inscreve → organização confirma e conclui
// → o indicador de impacto conta a conclusão. Depois, participação cidadã: propor, votar e atender.
test.describe.configure({ mode: 'serial' });

const sufixo = Date.now().toString().slice(-6);
const tituloOportunidade = `Curso E2E de Programação ${sufixo}`;
const tituloProposta = `Oficina E2E de Eletricista ${sufixo}`;
let emailOrg = '';
let emailCidadao = '';

test('organização se cadastra e publica uma oportunidade', async ({ page }) => {
  emailOrg = await cadastrarOrganizacao(page, `ONG E2E ${sufixo}`);
  await page.getByRole('link', { name: 'Nova oportunidade' }).first().click();
  await page.getByLabel('Título').fill(tituloOportunidade);
  await page.getByLabel('Descrição').fill('Criado automaticamente pelo teste E2E.');
  await page.getByLabel('Área').selectOption('Tecnologia');
  await page.getByLabel('Bairro').fill('Capão Redondo');
  await page.getByLabel('Vagas').fill('5');
  await page.getByRole('button', { name: 'Publicar oportunidade' }).click();
  await expect(page.getByRole('heading', { name: tituloOportunidade })).toBeVisible();
  await expect(page.getByText('5 vagas restantes')).toBeVisible();
});

test('cidadão se cadastra, vê a recomendação explicada e se inscreve', async ({ page }) => {
  emailCidadao = await cadastrarCidadao(page, `Cidadã E2E ${sufixo}`);
  const card = page.locator('article', { hasText: tituloOportunidade });
  await expect(card).toBeVisible();
  await expect(card.getByText('Por que recomendamos')).toBeVisible();
  await expect(card.getByText(/Combina com seu interesse em Tecnologia/)).toBeVisible();
  await expect(card.getByText(/Acontece no seu bairro/)).toBeVisible();

  await card.getByRole('link', { name: tituloOportunidade }).click();
  await page.getByRole('button', { name: 'Quero me inscrever' }).click();
  await expect(page.getByText('Inscrição feita!')).toBeVisible();
  await expect(page.getByText('Você está inscrito')).toBeVisible();
  await expect(page.getByText('4 vagas restantes')).toBeVisible();

  await page.goto('/minha-area?aba=inscricoes');
  await expect(page.getByRole('link', { name: tituloOportunidade })).toBeVisible();
  await expect(page.locator('li', { hasText: tituloOportunidade }).getByText('Pendente')).toBeVisible();
});

test('cidadão propõe uma qualificação para o bairro', async ({ page }) => {
  await entrar(page, emailCidadao);
  await page.goto('/propostas');
  await page.getByRole('button', { name: 'Nova proposta' }).click();
  await page.getByLabel('Título').fill(tituloProposta);
  await page.getByLabel('Por que é importante?').fill('Muita gente do bairro quer trabalhar com elétrica.');
  await page.getByRole('dialog').getByLabel('Área').selectOption('ConstrucaoCivil');
  await page.getByRole('dialog').getByLabel('Bairro').fill('Capão Redondo');
  await page.getByRole('button', { name: 'Publicar proposta' }).click();
  await expect(page.getByText('Proposta publicada!')).toBeVisible();
});

test('outro cidadão vota na proposta e não consegue votar duas vezes', async ({ page }) => {
  await cadastrarCidadao(page, `Vizinho E2E ${sufixo}`);
  await page.goto('/propostas?status=Aberta');
  const card = page.locator('article', { hasText: tituloProposta });
  await card.getByRole('button', { name: `Votar em ${tituloProposta}` }).click();
  await expect(page.getByText('Voto registrado!')).toBeVisible();
  await expect(card.getByRole('button', { name: /já votou/ })).toBeDisabled();
});

test('organização confirma e conclui a inscrição, e atende a proposta', async ({ page }) => {
  await entrar(page, emailOrg);
  await expect(page).toHaveURL(/\/organizacao$/);
  await page.locator('li', { hasText: tituloOportunidade }).getByRole('link', { name: 'Inscritos' }).click();

  const linha = page.locator('li', { hasText: `Cidadã E2E ${sufixo}` });
  await linha.getByRole('button', { name: 'Confirmar' }).click();
  await expect(linha.getByText('Confirmada')).toBeVisible();
  await linha.getByRole('button', { name: 'Concluir' }).click();
  await expect(linha.getByText('Concluída')).toBeVisible();

  await page.goto('/propostas?status=Aberta');
  await page.getByRole('link', { name: tituloProposta }).click();
  await page.getByRole('button', { name: 'Atender esta proposta' }).click();
  const dialogo = page.getByRole('dialog');
  await expect(dialogo.getByLabel('Título')).toHaveValue(tituloProposta);
  await dialogo.getByRole('button', { name: 'Publicar e atender' }).click();
  await expect(page.getByText('Proposta atendida!')).toBeVisible();
  await expect(page.getByRole('heading', { name: tituloProposta })).toBeVisible();
});

test('cidadão vê a inscrição concluída e a proposta atendida', async ({ page }) => {
  await entrar(page, emailCidadao);
  await page.goto('/minha-area?aba=inscricoes');
  await expect(page.locator('li', { hasText: tituloOportunidade }).getByText('Concluída')).toBeVisible();

  await page.goto('/minha-area?aba=propostas');
  const card = page.locator('article', { hasText: tituloProposta });
  await expect(card.getByText('Atendida')).toBeVisible();
  await expect(card.getByRole('link', { name: /Ver oportunidade criada/ })).toBeVisible();
  await sair(page);
});

test('admin vê o cidadão na lista sem dados sensíveis', async ({ page }) => {
  await entrar(page, 'admin@oportuniza.local');
  await expect(page).toHaveURL(/\/admin/);
  await expect(page.getByText('Taxa de conclusão')).toBeVisible();
  await page.getByRole('tab', { name: /Cidadãos/ }).click();
  await page.getByLabel('Bairro').selectOption('Capão Redondo');
  await expect(page.getByRole('table')).toBeVisible();
  await expect(page.getByRole('table')).not.toContainText('@');
});
