import { useState } from 'react';
import { CheckCircle2, Loader2, Search } from 'lucide-react';
import type { Endereco } from '../api/types';
import { enderecosApi } from '../api/services';
import { mensagemDeErro } from '../api/client';
import { formatarCep } from '../lib/format';
import { Input } from './ui/Field';

export const ENDERECO_VAZIO: Endereco = { cep: '', logradouro: '', numero: '', bairro: '', cidade: '', uf: '' };

interface Props {
  valor: Endereco;
  aoMudar: (e: Endereco) => void;
  erros?: Partial<Record<keyof Endereco, string>>;
}

/** Campos de endereço com preenchimento automático pelo ViaCEP (via API Oportuniza). */
export function EnderecoFields({ valor, aoMudar, erros = {} }: Props) {
  const [buscando, setBuscando] = useState(false);
  const [statusCep, setStatusCep] = useState<{ ok: boolean; texto: string } | null>(null);

  const mudar = (campo: keyof Endereco, v: string) => aoMudar({ ...valor, [campo]: v });

  async function buscarCep(cep: string) {
    if (cep.replace(/\D/g, '').length !== 8) return;
    setBuscando(true);
    setStatusCep(null);
    try {
      const r = await enderecosApi.consultar(cep);
      aoMudar({
        ...valor,
        cep: formatarCep(r.cep),
        logradouro: r.logradouro || valor.logradouro,
        bairro: r.bairro || valor.bairro,
        cidade: r.cidade || valor.cidade,
        uf: r.uf || valor.uf,
      });
      setStatusCep({ ok: true, texto: 'Endereço encontrado pelo ViaCEP' });
    } catch (e) {
      setStatusCep({ ok: false, texto: mensagemDeErro(e) });
    } finally {
      setBuscando(false);
    }
  }

  return (
    <div className="grid gap-4 sm:grid-cols-6">
      <Input
        className="sm:col-span-2"
        rotulo="CEP"
        inputMode="numeric"
        autoComplete="postal-code"
        placeholder="00000-000"
        value={valor.cep}
        erro={erros.cep ?? (statusCep && !statusCep.ok ? statusCep.texto : undefined)}
        ajuda={
          statusCep?.ok ? (
            <span className="flex items-center gap-1 text-emerald-700 dark:text-emerald-400">
              <CheckCircle2 className="size-3.5" aria-hidden /> {statusCep.texto}
            </span>
          ) : (
            'Preenchemos o resto para você'
          )
        }
        icone={buscando ? <Loader2 className="size-4 animate-spin" /> : <Search className="size-4" />}
        onChange={(e) => {
          const cep = formatarCep(e.target.value);
          mudar('cep', cep);
          if (cep.length === 9) void buscarCep(cep);
        }}
        required
      />
      <Input
        className="sm:col-span-4"
        rotulo="Rua / logradouro"
        autoComplete="address-line1"
        value={valor.logradouro}
        erro={erros.logradouro}
        maxLength={150}
        onChange={(e) => mudar('logradouro', e.target.value)}
        required
      />
      <Input
        className="sm:col-span-2"
        rotulo="Número"
        opcional
        value={valor.numero ?? ''}
        maxLength={10}
        onChange={(e) => mudar('numero', e.target.value)}
      />
      <Input
        className="sm:col-span-4"
        rotulo="Bairro"
        value={valor.bairro}
        erro={erros.bairro}
        maxLength={80}
        onChange={(e) => mudar('bairro', e.target.value)}
        required
      />
      <Input
        className="sm:col-span-4"
        rotulo="Cidade"
        autoComplete="address-level2"
        value={valor.cidade}
        erro={erros.cidade}
        maxLength={80}
        onChange={(e) => mudar('cidade', e.target.value)}
        required
      />
      <Input
        className="sm:col-span-2"
        rotulo="UF"
        autoComplete="address-level1"
        value={valor.uf}
        erro={erros.uf}
        maxLength={2}
        onChange={(e) => mudar('uf', e.target.value.toUpperCase().replace(/[^A-Z]/g, ''))}
        required
      />
    </div>
  );
}
