using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Common;
using Oportuniza.Domain.Exceptions;

namespace Oportuniza.Application.Common;

public static class RepositorioExtensions
{
    /// <summary>Carrega a raiz do agregado ou lança 404 com a mensagem informada.</summary>
    public static async Task<T> ObterObrigatorioAsync<T>(this IRepositorio<T> repositorio, long id,
        string mensagemNaoEncontrado, CancellationToken ct = default) where T : Entidade, IRaizDeAgregado =>
        await repositorio.ObterPorIdAsync(id, ct) ?? throw new RecursoNaoEncontradoException(mensagemNaoEncontrado);
}
