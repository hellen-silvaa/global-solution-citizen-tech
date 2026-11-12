import { useState, type FormEvent } from 'react';
import { Link, Navigate, useLocation, useNavigate } from 'react-router';
import { Eye, EyeOff, Lock, Mail, Zap } from 'lucide-react';
import { useAuth, rotaInicial } from '../auth/AuthProvider';
import { ApiError } from '../api/client';
import { AuthShell } from '../components/AuthShell';
import { Input } from '../components/ui/Field';
import { Button } from '../components/ui/Button';
import { useToast } from '../components/ui/Toast';
import { primeiroNome } from '../lib/format';

const DEMO = [
  { rotulo: 'Cidadã', email: 'ana@exemplo.local' },
  { rotulo: 'Organização', email: 'contato@recomecar.local' },
  { rotulo: 'Admin', email: 'admin@oportuniza.local' },
];

export default function LoginPage() {
  const { sessao, entrar } = useAuth();
  const navegar = useNavigate();
  const local = useLocation();
  const toast = useToast();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [verSenha, setVerSenha] = useState(false);
  const [erro, setErro] = useState<string>();
  const [enviando, setEnviando] = useState(false);

  const destino = (local.state as { de?: string } | null)?.de;
  if (sessao && !enviando) return <Navigate to={destino ?? rotaInicial(sessao.perfil)} replace />;

  async function enviar(e: FormEvent) {
    e.preventDefault();
    setErro(undefined);
    if (!email.trim() || !senha) {
      setErro('Informe e-mail e senha.');
      return;
    }
    setEnviando(true);
    try {
      const s = await entrar(email, senha);
      toast.sucesso(`Olá, ${primeiroNome(s.nome)}!`, 'Que bom ter você por aqui.');
      navegar(destino ?? rotaInicial(s.perfil), { replace: true });
    } catch (err) {
      setErro(err instanceof ApiError && err.status === 401 ? 'E-mail ou senha incorretos.' : (err as Error).message);
      setEnviando(false);
    }
  }

  return (
    <AuthShell
      titulo="Entrar"
      subtitulo={
        <>
          Novo por aqui? <Link to="/cadastro" className="font-semibold text-brand-600 hover:underline dark:text-brand-300">Crie sua conta grátis</Link>
        </>
      }
    >
      <form onSubmit={enviar} noValidate className="card space-y-5 p-6 sm:p-8">
        <Input rotulo="E-mail" type="email" autoComplete="email" icone={<Mail className="size-4" />} value={email} onChange={(e) => setEmail(e.target.value)} />
        <div className="relative">
          <Input
            rotulo="Senha"
            type={verSenha ? 'text' : 'password'}
            autoComplete="current-password"
            icone={<Lock className="size-4" />}
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
          />
          <button
            type="button"
            onClick={() => setVerSenha((v) => !v)}
            className="muted absolute right-3 top-[34px] rounded-lg p-1 hover:text-[var(--text)]"
            aria-label={verSenha ? 'Esconder senha' : 'Mostrar senha'}
          >
            {verSenha ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
          </button>
        </div>
        {erro && (
          <p role="alert" className="rounded-xl bg-rose-50 px-4 py-3 text-sm font-medium text-rose-700 dark:bg-rose-500/10 dark:text-rose-300">
            {erro}
          </p>
        )}
        <Button type="submit" tamanho="lg" className="w-full" carregando={enviando}>
          Entrar
        </Button>
        <p className="muted text-center text-xs">Por segurança, a conta é bloqueada por 15 minutos após 5 senhas erradas seguidas.</p>
      </form>

      <div className="surface-2 mt-6 rounded-2xl p-5">
        <p className="flex items-center gap-2 text-sm font-bold"><Zap className="size-4 text-sun-500" /> Acesso rápido de demonstração</p>
        <p className="muted mt-1 text-xs">Dados fictícios. Senha de todos: <code className="font-semibold">Senha@123</code></p>
        <div className="mt-3 flex flex-wrap gap-2">
          {DEMO.map((d) => (
            <button
              key={d.email}
              type="button"
              onClick={() => {
                setEmail(d.email);
                setSenha('Senha@123');
                setErro(undefined);
              }}
              className="rounded-xl border border-[var(--border)] bg-[var(--surface)] px-3 py-2 text-left text-xs transition hover:border-brand-400"
            >
              <span className="block font-bold">{d.rotulo}</span>
              <span className="muted">{d.email}</span>
            </button>
          ))}
        </div>
      </div>
    </AuthShell>
  );
}
