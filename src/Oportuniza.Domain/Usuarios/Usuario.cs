using Oportuniza.Domain.Common;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Domain.Usuarios;

/// <summary>
/// Conta de acesso à plataforma (raiz de agregado). Classe abstrata: todo usuário é um
/// <see cref="Cidadao"/>, uma <see cref="Organizacao"/> ou um <see cref="Administrador"/>.
/// Também protege a conta contra força bruta: após <see cref="MaximoTentativasLogin"/> falhas
/// seguidas, o login fica bloqueado por <see cref="DuracaoBloqueio"/>.
/// </summary>
public abstract class Usuario : Entidade, IRaizDeAgregado
{
    public const int MaximoTentativasLogin = 5;
    public static readonly TimeSpan DuracaoBloqueio = TimeSpan.FromMinutes(15);

    public string Nome { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public bool Ativo { get; private set; } = true;
    public Endereco? Endereco { get; private set; }
    public int TentativasLoginFalhas { get; private set; }
    public DateTime? BloqueadoAte { get; private set; }
    public DateTime? UltimoLoginEm { get; private set; }

    /// <summary>Perfil de acesso, definido pelo tipo concreto (polimorfismo).</summary>
    public abstract Perfil Perfil { get; }

    protected Usuario() { } // EF Core

    protected Usuario(string nome, Email email, string senhaHash, Endereco? endereco)
    {
        Nome = Guard.Texto(nome, "Nome", 120);
        Email = Guard.NaoNulo(email, "E-mail");
        SenhaHash = Guard.Texto(senhaHash, "Senha", 100);
        Endereco = endereco;
    }

    public void AtualizarDados(string? nome, Endereco? endereco)
    {
        if (!string.IsNullOrWhiteSpace(nome))
            Nome = Guard.Texto(nome, "Nome", 120);
        if (endereco is not null)
            Endereco = endereco;
    }

    public void AlterarSenha(string novaSenhaHash) => SenhaHash = Guard.Texto(novaSenhaHash, "Senha", 100);

    /// <summary>Exclusão lógica: o histórico de inscrições e votos é preservado.</summary>
    public void Desativar()
    {
        if (!Ativo)
            throw new ValidacaoException("Usuário já está inativo!");
        Ativo = false;
    }

    public void Reativar() => Ativo = true;

    // ---------- Proteção contra força bruta ----------

    public bool EstaBloqueado(DateTime agora) => BloqueadoAte.HasValue && BloqueadoAte.Value > agora;

    /// <summary>Conta uma senha errada; na 5ª seguida, bloqueia a conta por 15 minutos.</summary>
    public void RegistrarFalhaDeLogin(DateTime agora)
    {
        if (EstaBloqueado(agora))
            return;

        TentativasLoginFalhas++;
        if (TentativasLoginFalhas >= MaximoTentativasLogin)
        {
            BloqueadoAte = agora.Add(DuracaoBloqueio);
            TentativasLoginFalhas = 0;
        }
    }

    public void RegistrarLoginBemSucedido(DateTime agora)
    {
        TentativasLoginFalhas = 0;
        BloqueadoAte = null;
        UltimoLoginEm = agora;
    }
}

/// <summary>Administrador da plataforma: modera conteúdo e consulta os dados de todos.</summary>
public class Administrador : Usuario
{
    public override Perfil Perfil => Perfil.Admin;

    private Administrador() { } // EF Core

    public Administrador(string nome, Email email, string senhaHash)
        : base(nome, email, senhaHash, endereco: null) { }
}
