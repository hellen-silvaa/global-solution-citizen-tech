import { afterEach, describe, expect, it, vi } from 'vitest';
import { ApiError, configurarAutenticacao, mensagemDeErro, montarUrl, request } from './client';
import { oportunidadesApi } from './services';
import { mockarApi } from '../../test/utils';

afterEach(() => {
  vi.unstubAllGlobals();
  configurarAutenticacao(() => null, () => {});
});

describe('cliente HTTP', () => {
  it('monta a URL ignorando filtros vazios', () => {
    const url = montarUrl('/oportunidades', { area: 'Tecnologia', bairro: '', texto: undefined, gratuita: true, pagina: 2 });
    expect(url).toBe('http://127.0.0.1:5080/oportunidades?area=Tecnologia&gratuita=true&pagina=2');
  });

  it('envia o token como Bearer e o corpo em JSON', async () => {
    const chamadas = mockarApi([{ metodo: 'POST', url: /\/propostas$/, status: 201, corpo: { id: 9 } }]);
    configurarAutenticacao(() => 'abc', () => {});
    const r = await request<{ id: number }>('/propostas', { method: 'POST', body: { titulo: 'x' } });
    expect(r.id).toBe(9);
    expect(chamadas[0].headers.Authorization).toBe('Bearer abc');
    expect(chamadas[0].headers['Content-Type']).toBe('application/json');
    expect(chamadas[0].corpo).toEqual({ titulo: 'x' });
  });

  it('aceita 204 sem corpo', async () => {
    mockarApi([{ metodo: 'PATCH', url: /encerrar/, status: 204 }]);
    await expect(oportunidadesApi.encerrar(1)).resolves.toBeUndefined();
  });

  it('usa a mensagem de regra de negócio da API', async () => {
    mockarApi([{ metodo: 'POST', url: /inscricoes/, status: 400, corpo: { mensagem: 'Não há vagas disponíveis nesta oportunidade!' } }]);
    const erro = await oportunidadesApi.inscrever(1).catch((e) => e);
    expect(erro).toBeInstanceOf(ApiError);
    expect(erro.status).toBe(400);
    expect(erro.message).toBe('Não há vagas disponíveis nesta oportunidade!');
  });

  it('lê a lista de erros de validação por campo', async () => {
    mockarApi([{ metodo: 'POST', url: /cidadaos/, status: 400, corpo: [{ campo: 'Email', mensagem: 'E-mail inválido' }, { campo: 'Endereco.Cep', mensagem: 'CEP inválido' }] }]);
    const erro = (await request('/cidadaos', { method: 'POST', body: {} }).catch((e) => e)) as ApiError;
    expect(erro.message).toBe('E-mail inválido');
    expect(erro.erroDoCampo('email')).toBe('E-mail inválido');
    expect(erro.erroDoCampo('cep')).toBe('CEP inválido');
    expect(erro.erroDoCampo('nome')).toBeUndefined();
  });

  it('usa mensagem padrão quando a API não manda uma', async () => {
    mockarApi([{ url: /x/, status: 429, corpo: {} }]);
    const erro = (await request('/x').catch((e) => e)) as ApiError;
    expect(erro.message).toMatch(/Muitas tentativas/);
  });

  it('avisa a sessão quando um token é recusado (401)', async () => {
    mockarApi([{ url: /x/, status: 401 }]);
    const naoAutorizado = vi.fn();
    configurarAutenticacao(() => 'expirado', naoAutorizado);
    await expect(request('/x')).rejects.toBeInstanceOf(ApiError);
    expect(naoAutorizado).toHaveBeenCalledOnce();
  });

  it('não desloga quem nem estava logado', async () => {
    mockarApi([{ url: /x/, status: 401 }]);
    const naoAutorizado = vi.fn();
    configurarAutenticacao(() => null, naoAutorizado);
    await expect(request('/x')).rejects.toThrow();
    expect(naoAutorizado).not.toHaveBeenCalled();
  });

  it('traduz falha de rede para uma mensagem amigável', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')));
    const erro = (await request('/x').catch((e) => e)) as ApiError;
    expect(erro.status).toBe(0);
    expect(erro.message).toMatch(/API está no ar/);
  });

  it('aceita resposta de texto puro', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('ok', { status: 200 })));
    await expect(request('/x')).resolves.toBe('ok');
  });

  it('mensagemDeErro lida com qualquer coisa', () => {
    expect(mensagemDeErro(new Error('a'))).toBe('a');
    expect(mensagemDeErro('???')).toMatch(/Algo deu errado/);
  });
});
