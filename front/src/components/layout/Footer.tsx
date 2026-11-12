import { Link } from 'react-router';
import { Heart } from 'lucide-react';
import { LogoMarca } from './Logo';
import { API_URL } from '../../api/client';

const EQUIPE = ['Alexia Ramalho', 'Enzo Real', 'Gustavo Pasquini', 'Hellen Silva', 'Lorenzo Acquesta'];

export function Footer() {
  return (
    <footer className="mt-24 border-t border-[var(--border)] bg-[var(--surface)]">
      <div className="mx-auto grid max-w-7xl gap-10 px-4 py-12 sm:px-6 md:grid-cols-[1.4fr_1fr_1fr]">
        <div>
          <div className="flex items-center gap-2.5">
            <LogoMarca className="size-8" />
            <span className="font-display text-lg font-extrabold">Oportuniza</span>
          </div>
          <p className="muted mt-3 max-w-sm text-sm leading-relaxed">
            Qualificação gratuita perto de casa para quem mais precisa. Uma CitizenTech da Zona Sul de São Paulo.
          </p>
          <div className="mt-4 flex flex-wrap gap-1.5" aria-label="ODS conectados">
            {[
              ['4', 'bg-ods-4'],
              ['8', 'bg-ods-8'],
              ['10', 'bg-ods-10'],
              ['11', 'bg-ods-11'],
            ].map(([n, cor]) => (
              <span key={n} className={`${cor} grid size-8 place-items-center rounded-lg text-xs font-extrabold text-white`} title={`ODS ${n}`}>
                {n}
              </span>
            ))}
          </div>
        </div>
        <div>
          <h2 className="text-sm font-bold">Explore</h2>
          <ul className="muted mt-3 space-y-2 text-sm">
            <li><Link className="hover:text-brand-600" to="/oportunidades">Oportunidades</Link></li>
            <li><Link className="hover:text-brand-600" to="/propostas">Propostas da comunidade</Link></li>
            <li><Link className="hover:text-brand-600" to="/organizacoes">Organizações parceiras</Link></li>
            <li><Link className="hover:text-brand-600" to="/impacto">Painel de impacto</Link></li>
            <li><a className="hover:text-brand-600" href={`${API_URL}/swagger`} target="_blank" rel="noreferrer">Documentação da API</a></li>
          </ul>
        </div>
        <div>
          <h2 className="text-sm font-bold">Equipe</h2>
          <ul className="muted mt-3 space-y-1.5 text-sm">
            {EQUIPE.map((n) => (
              <li key={n}>{n}</li>
            ))}
          </ul>
        </div>
      </div>
      <div className="border-t border-[var(--border)]">
        <p className="muted mx-auto flex max-w-7xl flex-wrap items-center gap-1 px-4 py-5 text-xs sm:px-6">
          Global Solution FIAP 2026 · CitizenTech · feito com <Heart className="size-3.5 fill-rose-500 text-rose-500" aria-label="amor" /> na Zona Sul.
          Dados de demonstração são fictícios.
        </p>
      </div>
    </footer>
  );
}
