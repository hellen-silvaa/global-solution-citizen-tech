// Mesma política da API (SenhaForteAttribute): 8 a 64 caracteres, com letras e números.

export interface ForcaSenha {
  pontos: 0 | 1 | 2 | 3 | 4;
  rotulo: string;
  valida: boolean;
  requisitos: { texto: string; ok: boolean }[];
}

export function avaliarSenha(senha: string): ForcaSenha {
  const requisitos = [
    { texto: 'De 8 a 64 caracteres', ok: senha.length >= 8 && senha.length <= 64 },
    { texto: 'Pelo menos uma letra', ok: /\p{L}/u.test(senha) },
    { texto: 'Pelo menos um número', ok: /\d/.test(senha) },
    { texto: 'Sem espaço no início ou no fim', ok: senha.length > 0 && senha === senha.trim() },
  ];
  const valida = requisitos.every((r) => r.ok);

  const atendidos = requisitos.slice(0, 3).filter((r) => r.ok).length;
  let pontos: ForcaSenha['pontos'] = valida ? 3 : (Math.min(atendidos, 2) as 0 | 1 | 2);
  if (valida && (senha.length >= 12 || (/[^\p{L}\d\s]/u.test(senha) && /\p{Lu}/u.test(senha)))) pontos = 4;

  const rotulos = ['Muito fraca', 'Fraca', 'Razoável', 'Boa', 'Forte'];
  return { pontos, rotulo: senha ? rotulos[pontos] : '', valida, requisitos };
}
