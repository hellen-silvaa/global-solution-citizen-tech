import { describe, expect, it } from 'vitest';
import { avaliarSenha } from './senha';
import {
  dataParaApi,
  dataParaInput,
  diasAte,
  formatarCep,
  formatarData,
  formatarPeriodo,
  iniciais,
  plural,
  primeiroNome,
  rotuloPrazo,
} from './format';
import { AREA_INFO, humanizarTexto } from './labels';
import { AREAS } from '../api/types';

describe('avaliarSenha (mesma política da API)', () => {
  it.each([
    ['', false],
    ['curta1', false],
    ['somenteletras', false],
    ['12345678', false],
    [' Senha123', false],
    ['Senha123 ', false],
    ['senha123', true],
    ['Senha@123', true],
  ])('"%s" é válida? %s', (senha, valida) => {
    expect(avaliarSenha(senha).valida).toBe(valida);
  });

  it('rejeita senha com mais de 64 caracteres', () => {
    expect(avaliarSenha('a1'.repeat(33)).valida).toBe(false);
  });

  it('dá nota máxima para senha longa ou com símbolo e maiúscula', () => {
    expect(avaliarSenha('Senha@123').pontos).toBe(4);
    expect(avaliarSenha('umasenhamuitolonga1').pontos).toBe(4);
    expect(avaliarSenha('senha123').pontos).toBe(3);
  });

  it('nunca dá nota de senha válida a uma senha inválida', () => {
    expect(avaliarSenha('abcdefgh').pontos).toBeLessThan(3);
    expect(avaliarSenha('abc').rotulo).not.toBe('');
    expect(avaliarSenha('').rotulo).toBe('');
  });
});

describe('formatação', () => {
  it('trata datas da API como data local (sem fuso)', () => {
    expect(formatarData('2026-11-20T00:00:00')).toMatch(/20 nov\.? 2026|20 de nov/);
    expect(dataParaInput('2026-11-20T00:00:00')).toBe('2026-11-20');
    expect(dataParaApi('2026-11-20')).toBe('2026-11-20T00:00:00');
  });

  it('mostra período de um dia só como uma data', () => {
    expect(formatarPeriodo('2026-11-20T00:00:00', '2026-11-20T00:00:00')).toBe(formatarData('2026-11-20T00:00:00'));
    expect(formatarPeriodo('2026-11-20T00:00:00', '2026-12-20T00:00:00')).toContain(' a ');
  });

  it('calcula dias e rótulo de prazo', () => {
    const hoje = new Date(2026, 10, 10);
    expect(diasAte('2026-11-10T00:00:00', hoje)).toBe(0);
    expect(rotuloPrazo('2026-11-10T00:00:00', hoje)).toBe('Começa hoje');
    expect(rotuloPrazo('2026-11-11T00:00:00', hoje)).toBe('Começa amanhã');
    expect(rotuloPrazo('2026-11-20T00:00:00', hoje)).toBe('Começa em 10 dias');
    expect(rotuloPrazo('2026-11-01T00:00:00', hoje)).toBe('Já começou');
  });

  it('formata CEP enquanto a pessoa digita', () => {
    expect(formatarCep('01001')).toBe('01001');
    expect(formatarCep('01001000')).toBe('01001-000');
    expect(formatarCep('01.001-000xyz9')).toBe('01001-000');
  });

  it('gera iniciais ignorando o "(fictícia)" do seed', () => {
    expect(iniciais('Ana Souza (fictícia)')).toBe('AS');
    expect(iniciais('Bruno')).toBe('B');
    expect(iniciais('  ')).toBe('?');
    expect(primeiroNome('Ana Souza')).toBe('Ana');
  });

  it('pluraliza', () => {
    expect(plural(1, 'vaga')).toBe('1 vaga');
    expect(plural(2, 'vaga')).toBe('2 vagas');
    expect(plural(1000, 'voto')).toBe('1.000 votos');
  });
});

describe('rótulos', () => {
  it('toda área tem rótulo, ícone e cor', () => {
    for (const a of AREAS) {
      expect(AREA_INFO[a].rotulo).toBeTruthy();
      expect(AREA_INFO[a].icone).toBeTruthy();
    }
  });

  it('coloca acento nos nomes de enum vindos dos motivos da recomendação', () => {
    expect(humanizarTexto('Combina com seu interesse em Administracao')).toBe('Combina com seu interesse em Administração');
    expect(humanizarTexto('Acontece no seu bairro (Capão Redondo)')).toBe('Acontece no seu bairro (Capão Redondo)');
  });
});
