import type { ErroValidacao } from './types';

export const API_URL = (import.meta.env.VITE_API_URL ?? 'http://127.0.0.1:5080').replace(/\/$/, '');

/** Erro devolvido pela API, já traduzido para uma mensagem amigável. */
export class ApiError extends Error {
  readonly status: number;
  readonly erros: ErroValidacao[];

  constructor(status: number, mensagem: string, erros: ErroValidacao[] = []) {
    super(mensagem);
    this.name = 'ApiError';
    this.status = status;
    this.erros = erros;
  }

  /** Mensagem do campo (ex.: "Email"), para mostrar junto do input. */
  erroDoCampo(campo: string): string | undefined {
    const alvo = campo.toLowerCase();
    return this.erros.find((e) => e.campo?.toLowerCase() === alvo || e.campo?.toLowerCase().endsWith('.' + alvo))?.mensagem;
  }
}

const MENSAGENS_PADRAO: Record<number, string> = {
  400: 'Confira os dados informados.',
  401: 'Sua sessão expirou. Entre novamente.',
  403: 'Você não tem permissão para esta ação.',
  404: 'Não encontramos o que você procurou.',
  409: 'Alguém alterou estes dados ao mesmo tempo. Tente de novo.',
  429: 'Muitas tentativas. Aguarde um pouco e tente novamente.',
  503: 'Serviço externo indisponível no momento.',
};

type Query = Record<string, string | number | boolean | undefined | null>;

interface Opcoes {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  body?: unknown;
  query?: Query;
  signal?: AbortSignal;
}

let obterToken: () => string | null = () => null;
let aoNaoAutorizado: () => void = () => {};

/** Conecta o cliente HTTP à sessão (chamado pelo AuthProvider). */
export function configurarAutenticacao(token: () => string | null, naoAutorizado: () => void) {
  obterToken = token;
  aoNaoAutorizado = naoAutorizado;
}

export function montarUrl(caminho: string, query?: Query): string {
  const url = new URL(API_URL + caminho);
  if (query) {
    for (const [chave, valor] of Object.entries(query)) {
      if (valor !== undefined && valor !== null && valor !== '') url.searchParams.set(chave, String(valor));
    }
  }
  return url.toString();
}

async function lerCorpo(resposta: Response): Promise<unknown> {
  const texto = await resposta.text();
  if (!texto) return undefined;
  try {
    return JSON.parse(texto);
  } catch {
    return texto;
  }
}

export async function request<T>(caminho: string, opcoes: Opcoes = {}): Promise<T> {
  const headers: Record<string, string> = { Accept: 'application/json' };
  const token = obterToken();
  if (token) headers.Authorization = `Bearer ${token}`;
  if (opcoes.body !== undefined) headers['Content-Type'] = 'application/json';

  let resposta: Response;
  try {
    resposta = await fetch(montarUrl(caminho, opcoes.query), {
      method: opcoes.method ?? 'GET',
      headers,
      body: opcoes.body !== undefined ? JSON.stringify(opcoes.body) : undefined,
      signal: opcoes.signal,
    });
  } catch (e) {
    if (e instanceof DOMException && e.name === 'AbortError') throw e;
    throw new ApiError(0, 'Não foi possível falar com o servidor. Verifique se a API está no ar.');
  }

  const corpo = await lerCorpo(resposta);
  if (resposta.ok) return corpo as T;

  if (resposta.status === 401 && token) aoNaoAutorizado();

  if (Array.isArray(corpo)) {
    const erros = corpo as ErroValidacao[];
    throw new ApiError(resposta.status, erros[0]?.mensagem ?? MENSAGENS_PADRAO[400], erros);
  }
  const daApi = corpo && typeof corpo === 'object' && 'mensagem' in corpo && typeof corpo.mensagem === 'string' ? corpo.mensagem : '';
  const mensagem = daApi || MENSAGENS_PADRAO[resposta.status] || 'Algo deu errado. Tente novamente.';
  throw new ApiError(resposta.status, mensagem);
}

export function mensagemDeErro(erro: unknown): string {
  if (erro instanceof Error) return erro.message;
  return 'Algo deu errado. Tente novamente.';
}
