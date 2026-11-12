using Oportuniza.Domain.Exceptions;

namespace Oportuniza.Domain.ValueObjects;

/// <summary>
/// Intervalo de datas de uma oportunidade. Imutável e sempre válido:
/// o término nunca vem antes do início.
/// </summary>
public sealed class Periodo
{
    public DateTime Inicio { get; private set; }
    public DateTime Fim { get; private set; }

    private Periodo() { } // EF Core

    public Periodo(DateTime inicio, DateTime fim)
    {
        if (fim < inicio)
            throw new ValidacaoException("Data de término deve ser igual ou posterior à data de início!");
        Inicio = inicio;
        Fim = fim;
    }

    public bool JaComecouEm(DateTime momento) => momento >= Inicio;
}
