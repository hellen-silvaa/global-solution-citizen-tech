import { useState, type FormEvent } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { AREAS, type AreaConhecimento, type Proposta } from '../api/types';
import { propostasApi } from '../api/services';
import { AREA_INFO } from '../lib/labels';
import { Modal } from './ui/Modal';
import { Input, Select, Textarea } from './ui/Field';
import { Button } from './ui/Button';
import { useToast } from './ui/Toast';

interface Props {
  aberto: boolean;
  aoFechar: () => void;
  bairroPadrao?: string;
  aoCriar?: (p: Proposta) => void;
}

export function NovaPropostaModal({ aberto, aoFechar, bairroPadrao = '', aoCriar }: Props) {
  const [titulo, setTitulo] = useState('');
  const [descricao, setDescricao] = useState('');
  const [area, setArea] = useState<AreaConhecimento>('Tecnologia');
  const [bairro, setBairro] = useState(bairroPadrao);
  const [erros, setErros] = useState<Record<string, string>>({});
  const toast = useToast();
  const queryClient = useQueryClient();

  const criar = useMutation({
    mutationFn: () => propostasApi.criar({ titulo: titulo.trim(), descricao: descricao.trim(), area, bairro: bairro.trim() }),
    onSuccess: (p) => {
      toast.sucesso('Proposta publicada!', 'Agora é só chamar a vizinhança para votar.');
      void queryClient.invalidateQueries({ queryKey: ['propostas'] });
      void queryClient.invalidateQueries({ queryKey: ['impacto'] });
      setTitulo('');
      setDescricao('');
      aoFechar();
      aoCriar?.(p);
    },
    onError: (e) => setErros({ geral: e.message }),
  });

  function enviar(e: FormEvent) {
    e.preventDefault();
    const novos: Record<string, string> = {};
    if (!titulo.trim()) novos.titulo = 'Dê um título à proposta';
    if (!descricao.trim()) novos.descricao = 'Conte por que o bairro precisa disso';
    if (!bairro.trim()) novos.bairro = 'Informe o bairro';
    setErros(novos);
    if (Object.keys(novos).length === 0) criar.mutate();
  }

  return (
    <Modal aberto={aberto} aoFechar={aoFechar} titulo="Nova proposta para o bairro" descricao="Que qualificação está faltando perto de você?">
      <form id="form-proposta" onSubmit={enviar} noValidate className="space-y-4">
        <Input rotulo="Título" value={titulo} maxLength={120} erro={erros.titulo} onChange={(e) => setTitulo(e.target.value)} placeholder="Ex.: Curso de eletricista residencial" />
        <Textarea rotulo="Por que é importante?" value={descricao} maxLength={1000} erro={erros.descricao} onChange={(e) => setDescricao(e.target.value)} />
        <div className="grid gap-4 sm:grid-cols-2">
          <Select rotulo="Área" value={area} onChange={(e) => setArea(e.target.value as AreaConhecimento)}>
            {AREAS.map((a) => (
              <option key={a} value={a}>{AREA_INFO[a].rotulo}</option>
            ))}
          </Select>
          <Input rotulo="Bairro" value={bairro} maxLength={80} erro={erros.bairro} onChange={(e) => setBairro(e.target.value)} />
        </div>
        {erros.geral && <p role="alert" className="rounded-xl bg-rose-50 px-4 py-3 text-sm text-rose-700 dark:bg-rose-500/10 dark:text-rose-300">{erros.geral}</p>}
        <div className="flex justify-end gap-2 pt-2">
          <Button variante="secundario" onClick={aoFechar}>Cancelar</Button>
          <Button type="submit" carregando={criar.isPending}>Publicar proposta</Button>
        </div>
      </form>
    </Modal>
  );
}
