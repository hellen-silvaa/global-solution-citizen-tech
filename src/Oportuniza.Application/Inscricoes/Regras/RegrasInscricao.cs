using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Usuarios;

namespace Oportuniza.Application.Inscricoes.Regras;

/// <summary>
/// Regra de inscrição que depende de consulta ao banco (as que não dependem ficam no próprio agregado
/// <see cref="Oportunidade.Inscrever"/>). O InscricaoService recebe todas via DI e executa em ordem:
/// para criar uma regra nova basta implementar esta interface e registrá-la (princípio aberto/fechado).
/// </summary>
public interface IRegraInscricao
{
    Task ValidarAsync(Cidadao cidadao, Oportunidade oportunidade, CancellationToken ct = default);
}

/// <summary>RN-I3: o cidadão não pode ter outra inscrição ativa na mesma oportunidade.</summary>
public sealed class RegraInscricaoUnica(IInscricaoRepository inscricoes) : IRegraInscricao
{
    public async Task ValidarAsync(Cidadao cidadao, Oportunidade oportunidade, CancellationToken ct = default)
    {
        if (await inscricoes.ExisteInscricaoAtivaAsync(cidadao.Id, oportunidade.Id, ct))
            throw new ValidacaoException("Cidadão já possui inscrição ativa nesta oportunidade!");
    }
}

/// <summary>
/// RN-I7: cada cidadão pode ter no máximo <see cref="Limite"/> inscrições ativas ao mesmo tempo.
/// Evita que poucas pessoas segurem muitas vagas gratuitas e distribui melhor as oportunidades.
/// </summary>
public sealed class RegraLimiteInscricoesAtivas(IInscricaoRepository inscricoes) : IRegraInscricao
{
    public const int Limite = 3;

    public async Task ValidarAsync(Cidadao cidadao, Oportunidade oportunidade, CancellationToken ct = default)
    {
        if (await inscricoes.ContarAtivasDoCidadaoAsync(cidadao.Id, ct) >= Limite)
            throw new ValidacaoException($"Limite de {Limite} inscrições ativas atingido. Conclua ou cancele uma para se inscrever em outra!");
    }
}
