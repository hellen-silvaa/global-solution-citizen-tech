using Oportuniza.Domain.Inscricoes;
using Oportuniza.Domain.Enums;

namespace Oportuniza.Application.Dtos.Inscricoes;

/// <summary>Inscrição de um cidadão em uma oportunidade.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="CidadaoId">Cidadão inscrito.</param>
/// <param name="CidadaoNome">Nome do cidadão.</param>
/// <param name="OportunidadeId">Oportunidade escolhida.</param>
/// <param name="OportunidadeTitulo">Título da oportunidade.</param>
/// <param name="Status">Pendente, Confirmada, Concluida ou Cancelada.</param>
/// <param name="CriadaEm">Quando a inscrição foi feita (UTC).</param>
/// <param name="AtualizadaEm">Última mudança de status (UTC), se houver.</param>
public record DadosDetalhamentoInscricao(
    long Id, long CidadaoId, string? CidadaoNome, long OportunidadeId, string? OportunidadeTitulo,
    StatusInscricao Status, DateTime CriadaEm, DateTime? AtualizadaEm)
{
    public static DadosDetalhamentoInscricao De(Inscricao i) => new(
        i.Id, i.CidadaoId, i.Cidadao?.Nome, i.OportunidadeId, i.Oportunidade?.Titulo,
        i.Status, i.CriadoEm, i.AtualizadaEm);
}
