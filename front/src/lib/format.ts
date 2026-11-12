// A API trafega datas sem fuso ("2026-11-20T00:00:00"): tratamos como data local.

function paraData(iso: string): Date {
  const [data, hora = '00:00:00'] = iso.split('T');
  const [a, m, d] = data.split('-').map(Number);
  const [h, min] = hora.split(':').map(Number);
  return new Date(a, m - 1, d, h || 0, min || 0);
}

export function formatarData(iso: string): string {
  return paraData(iso).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' }).replace('.', '');
}

export function formatarDataCurta(iso: string): string {
  return paraData(iso).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' }).replace('.', '');
}

export function formatarPeriodo(inicio: string, fim: string): string {
  if (inicio.slice(0, 10) === fim.slice(0, 10)) return formatarData(inicio);
  return `${formatarDataCurta(inicio)} a ${formatarData(fim)}`;
}

/** Dias que faltam até a data (0 = hoje, negativo = já passou). */
export function diasAte(iso: string, hoje: Date = new Date()): number {
  const alvo = paraData(iso);
  const base = new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate());
  return Math.round((alvo.getTime() - base.getTime()) / 86_400_000);
}

export function rotuloPrazo(iso: string, hoje?: Date): string {
  const dias = diasAte(iso, hoje);
  if (dias < 0) return 'Já começou';
  if (dias === 0) return 'Começa hoje';
  if (dias === 1) return 'Começa amanhã';
  return `Começa em ${dias} dias`;
}

/** Valor de <input type="date"> → formato aceito pela API. */
export function dataParaApi(valor: string): string {
  return `${valor}T00:00:00`;
}

/** Data da API → valor de <input type="date">. */
export function dataParaInput(iso: string): string {
  return iso.slice(0, 10);
}

export function formatarCep(valor: string): string {
  const digitos = valor.replace(/\D/g, '').slice(0, 8);
  return digitos.length > 5 ? `${digitos.slice(0, 5)}-${digitos.slice(5)}` : digitos;
}

export function formatarNumero(n: number): string {
  return n.toLocaleString('pt-BR');
}

export function iniciais(nome: string): string {
  const partes = nome.replace(/\(.*?\)/g, '').trim().split(/\s+/).filter(Boolean);
  if (partes.length === 0) return '?';
  const primeira = partes[0][0];
  const ultima = partes.length > 1 ? partes[partes.length - 1][0] : '';
  return (primeira + ultima).toUpperCase();
}

export function primeiroNome(nome: string): string {
  return nome.trim().split(/\s+/)[0] ?? nome;
}

export function plural(n: number, singular: string, pluralForma = singular + 's'): string {
  return `${formatarNumero(n)} ${n === 1 ? singular : pluralForma}`;
}
