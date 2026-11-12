using Oportuniza.Application.Common;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Application.Ports.Persistence;

/// <summary>Acesso a qualquer tipo de usuário (usado no login e na unicidade do e-mail).</summary>
public interface IUsuarioRepository : IRepositorio<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken ct = default);
    Task<bool> EmailEmUsoAsync(Email email, CancellationToken ct = default);
}

public interface ICidadaoRepository : IRepositorio<Cidadao>
{
    Task<PaginaResultado<Cidadao>> ListarAsync(string? bairro, Paginacao paginacao, CancellationToken ct = default);
}

public interface IOrganizacaoRepository : IRepositorio<Organizacao>
{
    Task<PaginaResultado<Organizacao>> ListarAsync(TipoOrganizacao? tipo, Paginacao paginacao, CancellationToken ct = default);
}
