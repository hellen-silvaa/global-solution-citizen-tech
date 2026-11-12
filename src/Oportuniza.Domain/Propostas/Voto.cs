using Oportuniza.Domain.Common;
using Oportuniza.Domain.Usuarios;

namespace Oportuniza.Domain.Propostas;

/// <summary>Apoio de um cidadão a uma proposta. Entidade interna do agregado <see cref="Proposta"/>.</summary>
public class Voto : Entidade
{
    public long PropostaId { get; private set; }
    public Proposta Proposta { get; private set; } = null!;
    public long CidadaoId { get; private set; }
    public Cidadao Cidadao { get; private set; } = null!;

    private Voto() { } // EF Core

    internal Voto(Proposta proposta, Cidadao cidadao)
    {
        Proposta = proposta;
        PropostaId = proposta.Id;
        Cidadao = cidadao;
        CidadaoId = cidadao.Id;
    }
}
