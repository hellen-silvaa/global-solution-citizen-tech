import { useEffect, useRef, useState } from 'react';
import { animate, useInView, useReducedMotion } from 'motion/react';

/** Número que "conta" até o valor quando aparece na tela. */
export function AnimatedNumber({ valor, casas = 0, sufixo = '' }: { valor: number; casas?: number; sufixo?: string }) {
  const ref = useRef<HTMLSpanElement>(null);
  const visivel = useInView(ref, { once: true, margin: '-40px' });
  const reduzir = useReducedMotion();
  const [exibido, setExibido] = useState(reduzir ? valor : 0);

  useEffect(() => {
    if (reduzir) {
      setExibido(valor);
      return;
    }
    if (!visivel) return;
    const controle = animate(0, valor, { duration: 1.4, ease: [0.16, 1, 0.3, 1], onUpdate: setExibido });
    return () => controle.stop();
  }, [valor, visivel, reduzir]);

  return (
    <span ref={ref} className="tabular-nums">
      {exibido.toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas })}
      {sufixo}
    </span>
  );
}
