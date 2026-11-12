import { useCallback, useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { propostasApi } from '../api/services';
import { ApiError } from '../api/client';
import { useAuth } from '../auth/AuthProvider';
import { useToast } from '../components/ui/Toast';

// A API não informa em quais propostas o cidadão já votou; guardamos localmente
// só para pintar o botão. A regra (um voto por pessoa, RN-P1) continua na API.
function chave(usuarioId: number) {
  return `oportuniza.votos.${usuarioId}`;
}

function ler(usuarioId: number | undefined): number[] {
  if (!usuarioId) return [];
  try {
    return JSON.parse(localStorage.getItem(chave(usuarioId)) ?? '[]') as number[];
  } catch {
    return [];
  }
}

export function useVotos() {
  const { sessao } = useAuth();
  const queryClient = useQueryClient();
  const toast = useToast();
  const usuarioId = sessao?.usuarioId;
  const [votados, setVotados] = useState<number[]>(() => ler(usuarioId));
  const podeVotar = sessao?.perfil === 'Cidadao';

  const marcar = useCallback(
    (id: number) => {
      if (!usuarioId) return;
      setVotados((atual) => {
        const novo = atual.includes(id) ? atual : [...atual, id];
        try {
          localStorage.setItem(chave(usuarioId), JSON.stringify(novo));
        } catch {
          /* ignora */
        }
        return novo;
      });
    },
    [usuarioId],
  );

  const mutacao = useMutation({
    mutationFn: (id: number) => propostasApi.votar(id),
    onSuccess: (proposta) => {
      marcar(proposta.id);
      toast.sucesso('Voto registrado!', `"${proposta.titulo}" agora tem ${proposta.totalVotos} ${proposta.totalVotos === 1 ? 'voto' : 'votos'}.`);
      void queryClient.invalidateQueries({ queryKey: ['propostas'] });
      void queryClient.invalidateQueries({ queryKey: ['proposta', proposta.id] });
      void queryClient.invalidateQueries({ queryKey: ['impacto'] });
    },
    onError: (erro, id) => {
      if (erro instanceof ApiError && /já votou/i.test(erro.message)) marcar(id);
      toast.erro('Não foi possível votar', erro.message);
    },
  });

  return {
    podeVotar,
    jaVotou: (id: number) => votados.includes(id),
    votar: (id: number) => mutacao.mutate(id),
    votandoId: mutacao.isPending ? mutacao.variables : undefined,
  };
}
