using Microsoft.EntityFrameworkCore;
using Oportuniza.Application.Common;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Infrastructure.Persistence.Repositories;

public class UsuarioRepository(OportunizaDbContext db) : RepositorioBase<Usuario>(db), IUsuarioRepository
{
    public Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken ct = default) =>
        Db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> EmailEmUsoAsync(Email email, CancellationToken ct = default) =>
        Db.Usuarios.AnyAsync(u => u.Email == email, ct);
}

public class CidadaoRepository(OportunizaDbContext db) : RepositorioBase<Cidadao>(db), ICidadaoRepository
{
    public Task<PaginaResultado<Cidadao>> ListarAsync(string? bairro, Paginacao paginacao, CancellationToken ct = default)
    {
        var query = Db.Cidadaos.AsNoTracking().Where(c => c.Ativo);
        if (!string.IsNullOrWhiteSpace(bairro))
            query = query.Where(c => c.Endereco!.Bairro == bairro.Trim());
        return query.OrderBy(c => c.Nome).PaginarAsync(paginacao, ct);
    }
}

public class OrganizacaoRepository(OportunizaDbContext db) : RepositorioBase<Organizacao>(db), IOrganizacaoRepository
{
    public Task<PaginaResultado<Organizacao>> ListarAsync(TipoOrganizacao? tipo, Paginacao paginacao, CancellationToken ct = default)
    {
        var query = Db.Organizacoes.AsNoTracking().Where(o => o.Ativo);
        if (tipo.HasValue)
            query = query.Where(o => o.TipoOrganizacao == tipo.Value);
        return query.OrderBy(o => o.Nome).PaginarAsync(paginacao, ct);
    }
}
