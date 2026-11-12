import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router';
import { AnimatePresence, motion } from 'motion/react';
import { Building2, UserRound } from 'lucide-react';
import { cidadaosApi, organizacoesApi } from '../api/services';
import { ApiError } from '../api/client';
import {
  ESCOLARIDADES,
  TIPOS_ORGANIZACAO,
  type AreaConhecimento,
  type Endereco,
  type Escolaridade,
  type TipoOrganizacao,
} from '../api/types';
import { ESCOLARIDADE_ROTULO, TIPO_ORGANIZACAO_ROTULO } from '../lib/labels';
import { avaliarSenha } from '../lib/senha';
import { dataParaApi } from '../lib/format';
import { useAuth, rotaInicial } from '../auth/AuthProvider';
import { AuthShell } from '../components/AuthShell';
import { Input, Select, Textarea } from '../components/ui/Field';
import { Button } from '../components/ui/Button';
import { PasswordStrength } from '../components/PasswordStrength';
import { AreasPicker } from '../components/AreasPicker';
import { ENDERECO_VAZIO, EnderecoFields } from '../components/EnderecoFields';
import { useToast } from '../components/ui/Toast';
import { cn } from '../lib/cn';

type Tipo = 'cidadao' | 'organizacao';
type Erros = Record<string, string>;

const CAMPOS_ENDERECO = ['cep', 'logradouro', 'bairro', 'cidade', 'uf'] as const;

export function validarComum(dados: { nome: string; email: string; senha: string; confirmacao: string; endereco: Endereco }): Erros {
  const erros: Erros = {};
  if (!dados.nome.trim()) erros.nome = 'Informe o nome';
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(dados.email.trim())) erros.email = 'Informe um e-mail válido';
  if (!avaliarSenha(dados.senha).valida) erros.senha = 'A senha não atende aos requisitos';
  if (dados.senha !== dados.confirmacao) erros.confirmacao = 'As senhas não conferem';
  if (!/^\d{5}-?\d{3}$/.test(dados.endereco.cep)) erros.cep = 'CEP inválido';
  if (!dados.endereco.logradouro.trim()) erros.logradouro = 'Informe a rua';
  if (!dados.endereco.bairro.trim()) erros.bairro = 'Informe o bairro';
  if (!dados.endereco.cidade.trim()) erros.cidade = 'Informe a cidade';
  if (!/^[A-Z]{2}$/.test(dados.endereco.uf)) erros.uf = 'UF com 2 letras';
  return erros;
}

/** Traduz os erros de validação da API ({campo, mensagem}) para o formulário. */
function errosDaApi(e: ApiError): Erros {
  const r: Erros = {};
  for (const { campo, mensagem } of e.erros) {
    const chave = campo.split('.').pop()!.replace(/^\w/, (c) => c.toLowerCase());
    r[chave] ??= mensagem;
  }
  return r;
}

export default function CadastroPage() {
  const [params, setParams] = useSearchParams();
  const tipo: Tipo = params.get('perfil') === 'organizacao' ? 'organizacao' : 'cidadao';
  const { entrar } = useAuth();
  const navegar = useNavigate();
  const toast = useToast();

  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [confirmacao, setConfirmacao] = useState('');
  const [endereco, setEndereco] = useState<Endereco>(ENDERECO_VAZIO);
  const [nascimento, setNascimento] = useState('');
  const [escolaridade, setEscolaridade] = useState<Escolaridade>('MedioCompleto');
  const [areas, setAreas] = useState<AreaConhecimento[]>([]);
  const [tipoOrg, setTipoOrg] = useState<TipoOrganizacao>('Ong');
  const [descricao, setDescricao] = useState('');
  const [site, setSite] = useState('');
  const [aceite, setAceite] = useState(false);
  const [erros, setErros] = useState<Erros>({});
  const [enviando, setEnviando] = useState(false);

  async function enviar(e: FormEvent) {
    e.preventDefault();
    const novos = validarComum({ nome, email, senha, confirmacao, endereco });
    if (tipo === 'cidadao') {
      if (!nascimento) novos.dataNascimento = 'Informe sua data de nascimento';
      else if (new Date(nascimento) >= new Date()) novos.dataNascimento = 'A data deve estar no passado';
      if (areas.length === 0) novos.areasInteresse = 'Escolha pelo menos uma área';
    } else {
      if (!descricao.trim()) novos.descricao = 'Conte um pouco sobre a organização';
      if (site && !/^https?:\/\/\S+\.\S+/.test(site)) novos.site = 'Use um endereço começando com http:// ou https://';
    }
    if (!aceite) novos.aceite = 'É preciso concordar para continuar';
    setErros(novos);
    if (Object.keys(novos).length > 0) {
      document.querySelector<HTMLElement>('[aria-invalid="true"]')?.focus();
      return;
    }

    setEnviando(true);
    const end = { ...endereco, numero: endereco.numero || null };
    try {
      if (tipo === 'cidadao') {
        await cidadaosApi.cadastrar({
          nome: nome.trim(),
          email: email.trim(),
          senha,
          dataNascimento: dataParaApi(nascimento),
          escolaridade,
          areasInteresse: areas,
          endereco: end,
        });
      } else {
        await organizacoesApi.cadastrar({ nome: nome.trim(), email: email.trim(), senha, tipo: tipoOrg, descricao: descricao.trim(), site: site.trim() || null, endereco: end });
      }
      const s = await entrar(email, senha);
      toast.sucesso('Conta criada! 🎉', tipo === 'cidadao' ? 'Separamos recomendações para você.' : 'Já pode publicar sua primeira oportunidade.');
      navegar(rotaInicial(s.perfil), { replace: true });
    } catch (err) {
      if (err instanceof ApiError && err.erros.length > 0) setErros(errosDaApi(err));
      else setErros({ geral: (err as Error).message });
      setEnviando(false);
    }
  }

  const errosEndereco = Object.fromEntries(CAMPOS_ENDERECO.filter((c) => erros[c]).map((c) => [c, erros[c]]));

  return (
    <AuthShell
      titulo="Criar conta"
      subtitulo={
        <>
          Já tem conta? <Link to="/entrar" className="font-semibold text-brand-600 hover:underline dark:text-brand-300">Entrar</Link>
        </>
      }
    >
      <div className="surface-2 mb-6 grid grid-cols-2 gap-1 rounded-2xl p-1" role="tablist" aria-label="Tipo de conta">
        {([
          ['cidadao', 'Quero me qualificar', UserRound],
          ['organizacao', 'Sou uma organização', Building2],
        ] as const).map(([t, rotulo, Icone]) => (
          <button
            key={t}
            type="button"
            role="tab"
            aria-selected={tipo === t}
            onClick={() => {
              setParams(t === 'organizacao' ? { perfil: t } : {}, { replace: true });
              setErros({});
            }}
            className={cn('relative flex items-center justify-center gap-2 rounded-xl px-3 py-3 text-sm font-semibold transition', tipo === t ? 'text-white' : 'muted hover:text-[var(--text)]')}
          >
            {tipo === t && <motion.span layoutId="aba-cadastro" className="absolute inset-0 rounded-xl bg-brand-600 shadow-lg shadow-brand-600/30" />}
            <Icone className="relative size-4" />
            <span className="relative">{rotulo}</span>
          </button>
        ))}
      </div>

      <form onSubmit={enviar} noValidate className="card space-y-8 p-6 sm:p-8">
        <section className="space-y-4">
          <h2 className="text-lg font-bold">{tipo === 'cidadao' ? 'Sobre você' : 'Sobre a organização'}</h2>
          <Input rotulo={tipo === 'cidadao' ? 'Nome completo' : 'Nome da organização'} autoComplete={tipo === 'cidadao' ? 'name' : 'organization'} value={nome} maxLength={120} erro={erros.nome} onChange={(e) => setNome(e.target.value)} />
          <Input rotulo="E-mail" type="email" autoComplete="email" value={email} maxLength={120} erro={erros.email} onChange={(e) => setEmail(e.target.value)} />
          <AnimatePresence mode="wait" initial={false}>
            {tipo === 'cidadao' ? (
              <motion.div key="c" initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }} className="space-y-4">
                <div className="grid gap-4 sm:grid-cols-2">
                  <Input rotulo="Data de nascimento" type="date" autoComplete="bday" value={nascimento} erro={erros.dataNascimento} onChange={(e) => setNascimento(e.target.value)} />
                  <Select rotulo="Escolaridade" value={escolaridade} onChange={(e) => setEscolaridade(e.target.value as Escolaridade)}>
                    {ESCOLARIDADES.map((x) => <option key={x} value={x}>{ESCOLARIDADE_ROTULO[x]}</option>)}
                  </Select>
                </div>
                <AreasPicker valor={areas} aoMudar={setAreas} erro={erros.areasInteresse} />
              </motion.div>
            ) : (
              <motion.div key="o" initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }} className="space-y-4">
                <Select rotulo="Tipo de organização" value={tipoOrg} onChange={(e) => setTipoOrg(e.target.value as TipoOrganizacao)}>
                  {TIPOS_ORGANIZACAO.map((x) => <option key={x} value={x}>{TIPO_ORGANIZACAO_ROTULO[x]}</option>)}
                </Select>
                <Textarea rotulo="Descrição" value={descricao} maxLength={500} erro={erros.descricao} onChange={(e) => setDescricao(e.target.value)} placeholder="O que a organização faz e para quem" />
                <Input rotulo="Site" opcional type="url" placeholder="https://" value={site} maxLength={200} erro={erros.site} onChange={(e) => setSite(e.target.value)} />
              </motion.div>
            )}
          </AnimatePresence>
        </section>

        <section className="space-y-4">
          <h2 className="text-lg font-bold">Endereço</h2>
          <EnderecoFields valor={endereco} aoMudar={setEndereco} erros={errosEndereco} />
        </section>

        <section className="space-y-4">
          <h2 className="text-lg font-bold">Senha</h2>
          <div className="grid gap-4 sm:grid-cols-2">
            <Input rotulo="Senha" type="password" autoComplete="new-password" value={senha} maxLength={64} erro={erros.senha} onChange={(e) => setSenha(e.target.value)} />
            <Input rotulo="Confirme a senha" type="password" autoComplete="new-password" value={confirmacao} maxLength={64} erro={erros.confirmacao} onChange={(e) => setConfirmacao(e.target.value)} />
          </div>
          <PasswordStrength senha={senha} />
        </section>

        <div>
          <label className="flex cursor-pointer items-start gap-3 text-sm">
            <input type="checkbox" className="mt-0.5 size-5 accent-brand-600" checked={aceite} onChange={(e) => setAceite(e.target.checked)} aria-invalid={erros.aceite ? true : undefined} />
            <span>
              Concordo com o uso dos meus dados apenas para recomendar e gerenciar oportunidades, conforme a LGPD. Posso desativar minha conta quando quiser.
            </span>
          </label>
          {erros.aceite && <p role="alert" className="mt-1 text-sm font-medium text-rose-600 dark:text-rose-400">{erros.aceite}</p>}
        </div>

        {erros.geral && <p role="alert" className="rounded-xl bg-rose-50 px-4 py-3 text-sm font-medium text-rose-700 dark:bg-rose-500/10 dark:text-rose-300">{erros.geral}</p>}

        <Button type="submit" tamanho="lg" className="w-full" carregando={enviando}>
          Criar minha conta
        </Button>
      </form>
    </AuthShell>
  );
}
