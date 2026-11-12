import { useCallback, useEffect, useState } from 'react';

export type Tema = 'light' | 'dark';
const CHAVE = 'oportuniza.tema';

function temaInicial(): Tema {
  return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
}

export function useTheme() {
  const [tema, setTema] = useState<Tema>(temaInicial);

  useEffect(() => {
    document.documentElement.classList.toggle('dark', tema === 'dark');
  }, [tema]);

  const alternar = useCallback(() => {
    setTema((atual) => {
      const novo = atual === 'dark' ? 'light' : 'dark';
      try {
        localStorage.setItem(CHAVE, novo);
      } catch {
        /* sem armazenamento: vale só nesta visita */
      }
      return novo;
    });
  }, []);

  return { tema, alternar };
}
