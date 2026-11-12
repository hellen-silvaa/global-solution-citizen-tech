import { Link } from 'react-router';

export function LogoMarca({ className = 'size-9' }: { className?: string }) {
  return (
    <svg viewBox="0 0 64 64" className={className} aria-hidden>
      <defs>
        <linearGradient id="logo-g" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" stopColor="#6366f1" />
          <stop offset="0.55" stopColor="#a855f7" />
          <stop offset="1" stopColor="#f97316" />
        </linearGradient>
      </defs>
      <rect width="64" height="64" rx="18" fill="url(#logo-g)" />
      <circle cx="32" cy="35" r="13" fill="none" stroke="#fff" strokeWidth="6" />
      <path d="M32 9v13" stroke="#fff" strokeWidth="6" strokeLinecap="round" />
      <circle cx="32" cy="35" r="4" fill="#fff" />
    </svg>
  );
}

export function Logo() {
  return (
    <Link to="/" className="group flex items-center gap-2.5" aria-label="Oportuniza, página inicial">
      <LogoMarca className="size-9 transition-transform duration-300 group-hover:rotate-[-8deg] group-hover:scale-105" />
      <span className="font-display text-xl font-extrabold tracking-tight">
        Oportuniza
      </span>
    </Link>
  );
}
