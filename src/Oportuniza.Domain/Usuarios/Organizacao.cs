using Oportuniza.Domain.Common;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Domain.Usuarios;

/// <summary>
/// Quem oferece qualificação: ONG, escola técnica, empresa, poder público ou coletivo.
/// Publica oportunidades e pode atender propostas da comunidade.
/// </summary>
public class Organizacao : Usuario
{
    public TipoOrganizacao TipoOrganizacao { get; private set; }
    public string Descricao { get; private set; } = null!;
    public string? Site { get; private set; }

    public override Perfil Perfil => Perfil.Organizacao;

    private Organizacao() { } // EF Core

    public Organizacao(string nome, Email email, string senhaHash, Endereco endereco,
        TipoOrganizacao tipo, string descricao, string? site)
        : base(nome, email, senhaHash, Guard.NaoNulo(endereco, "Endereço"))
    {
        TipoOrganizacao = tipo;
        Descricao = Guard.Texto(descricao, "Descrição", 500);
        Site = ValidarSite(site);
    }

    public void AtualizarApresentacao(string? descricao, string? site)
    {
        if (!string.IsNullOrWhiteSpace(descricao))
            Descricao = Guard.Texto(descricao, "Descrição", 500);
        if (site is not null)
            Site = ValidarSite(site);
    }

    /// <summary>
    /// Só aceita links http/https absolutos. Bloqueia esquemas perigosos como "javascript:" e "data:",
    /// que poderiam virar XSS se um front-end exibir o site como link.
    /// </summary>
    private static string? ValidarSite(string? site)
    {
        var limpo = Guard.TextoOpcional(site, "Site", 200);
        if (limpo is null)
            return null;
        if (!Uri.TryCreate(limpo, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ValidacaoException("Site deve ser um endereço http ou https válido!");
        return limpo;
    }
}
