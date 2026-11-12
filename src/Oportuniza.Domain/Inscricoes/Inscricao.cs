using Oportuniza.Domain.Common;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Usuarios;

namespace Oportuniza.Domain.Inscricoes;

/// <summary>
/// Inscrição de um cidadão em uma oportunidade (raiz de agregado).
/// Máquina de estados: Pendente → Confirmada → Concluída; Pendente/Confirmada → Cancelada.
/// Só é criada por <see cref="Oportunidade.Inscrever"/>, que reserva a vaga.
/// </summary>
public class Inscricao : Entidade, IRaizDeAgregado
{
    public long CidadaoId { get; private set; }
    public Cidadao Cidadao { get; private set; } = null!;
    public long OportunidadeId { get; private set; }
    public Oportunidade Oportunidade { get; private set; } = null!;
    public StatusInscricao Status { get; private set; } = StatusInscricao.Pendente;
    public DateTime? AtualizadaEm { get; private set; }

    /// <summary>Inscrição que ainda ocupa vaga (RN-I3).</summary>
    public bool EstaAtiva => Status is StatusInscricao.Pendente or StatusInscricao.Confirmada;

    private Inscricao() { } // EF Core

    internal Inscricao(Cidadao cidadao, Oportunidade oportunidade)
    {
        Cidadao = cidadao;
        CidadaoId = cidadao.Id;
        Oportunidade = oportunidade;
        OportunidadeId = oportunidade.Id;
    }

    public bool PertenceA(long cidadaoId) => CidadaoId == cidadaoId;

    /// <summary>Quem pode ver/cancelar: o próprio cidadão ou a organização responsável.</summary>
    public bool EnvolveUsuario(long usuarioId) => PertenceA(usuarioId) || Oportunidade.PertenceA(usuarioId);

    public void Confirmar() => Transicionar(StatusInscricao.Confirmada, StatusInscricao.Pendente);

    public void Concluir() => Transicionar(StatusInscricao.Concluida, StatusInscricao.Confirmada);

    /// <summary>RN-I6: cancelamento só antes do início; a vaga volta para a oportunidade.</summary>
    public void Cancelar(DateTime agora)
    {
        if (Oportunidade.Periodo.JaComecouEm(agora))
            throw new ValidacaoException("Inscrição só pode ser cancelada antes do início da oportunidade!");
        Transicionar(StatusInscricao.Cancelada, StatusInscricao.Pendente, StatusInscricao.Confirmada);
        Oportunidade.LiberarVaga();
    }

    private void Transicionar(StatusInscricao destino, params StatusInscricao[] origensPermitidas)
    {
        if (!origensPermitidas.Contains(Status))
            throw new ValidacaoException($"Não é possível mudar a inscrição de {Status} para {destino}!");
        Status = destino;
        AtualizadaEm = DateTime.UtcNow;
    }
}
