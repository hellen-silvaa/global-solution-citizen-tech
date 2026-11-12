using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Domain;

public class UsuarioTests
{
    [Fact]
    public void Perfil_DependeDoTipoConcreto() // polimorfismo
    {
        Usuario[] usuarios = [Fabrica.Cidadao(), Fabrica.Organizacao(), Fabrica.Admin()];

        Assert.Equal([Perfil.Cidadao, Perfil.Organizacao, Perfil.Admin], usuarios.Select(u => u.Perfil));
    }

    [Fact]
    public void NovoUsuario_ComecaAtivoESemBloqueio()
    {
        var admin = Fabrica.Admin();

        Assert.True(admin.Ativo);
        Assert.Null(admin.Endereco);
        Assert.Equal(0, admin.TentativasLoginFalhas);
        Assert.False(admin.EstaBloqueado(Fabrica.Agora));
    }

    [Theory]
    [InlineData("", "hash")]
    [InlineData("Nome", "")]
    public void Construtor_CamposObrigatorios_LancaValidacao(string nome, string hash) =>
        Assert.Throws<ValidacaoException>(() => new Administrador(nome, Fabrica.Email(), hash));

    [Fact]
    public void Construtor_SemEmail_LancaValidacao() =>
        Assert.Throws<ValidacaoException>(() => new Administrador("Admin", null!, "hash"));

    [Fact]
    public void Construtor_NomeMuitoLongo_LancaValidacao() =>
        Assert.Throws<ValidacaoException>(() => new Administrador(new string('a', 121), Fabrica.Email(), "hash"));

    [Fact]
    public void AtualizarDados_SoAlteraOQueFoiInformado()
    {
        var cidadao = Fabrica.Cidadao();
        var enderecoOriginal = cidadao.Endereco;

        cidadao.AtualizarDados(null, null);
        Assert.Equal("Cidadã Teste", cidadao.Nome);
        Assert.Same(enderecoOriginal, cidadao.Endereco);

        cidadao.AtualizarDados("  Novo Nome ", Fabrica.Endereco("Centro"));
        Assert.Equal("Novo Nome", cidadao.Nome);
        Assert.Equal("Centro", cidadao.Endereco!.Bairro);
    }

    [Fact]
    public void AlterarSenha_TrocaOHash()
    {
        var admin = Fabrica.Admin();
        admin.AlterarSenha("novo-hash");
        Assert.Equal("novo-hash", admin.SenhaHash);
        Assert.Throws<ValidacaoException>(() => admin.AlterarSenha(" "));
    }

    [Fact]
    public void Desativar_DuasVezes_LancaValidacao_EReativarVolta()
    {
        var cidadao = Fabrica.Cidadao();
        cidadao.Desativar();

        Assert.False(cidadao.Ativo);
        Assert.Throws<ValidacaoException>(cidadao.Desativar);

        cidadao.Reativar();
        Assert.True(cidadao.Ativo);
    }

    [Fact]
    public void QuintaFalhaDeLogin_BloqueiaPorQuinzeMinutos()
    {
        var usuario = Fabrica.Cidadao();

        for (var i = 0; i < Usuario.MaximoTentativasLogin - 1; i++)
            usuario.RegistrarFalhaDeLogin(Fabrica.Agora);
        Assert.False(usuario.EstaBloqueado(Fabrica.Agora));
        Assert.Equal(4, usuario.TentativasLoginFalhas);

        usuario.RegistrarFalhaDeLogin(Fabrica.Agora);

        Assert.True(usuario.EstaBloqueado(Fabrica.Agora));
        Assert.True(usuario.EstaBloqueado(Fabrica.Agora.AddMinutes(14)));
        Assert.False(usuario.EstaBloqueado(Fabrica.Agora.Add(Usuario.DuracaoBloqueio)));
        Assert.Equal(0, usuario.TentativasLoginFalhas);
    }

    [Fact]
    public void FalhaDuranteBloqueio_NaoEstendeOBloqueio()
    {
        var usuario = Fabrica.Cidadao();
        for (var i = 0; i < Usuario.MaximoTentativasLogin; i++)
            usuario.RegistrarFalhaDeLogin(Fabrica.Agora);
        var bloqueadoAte = usuario.BloqueadoAte;

        usuario.RegistrarFalhaDeLogin(Fabrica.Agora.AddMinutes(5));

        Assert.Equal(bloqueadoAte, usuario.BloqueadoAte);
        Assert.Equal(0, usuario.TentativasLoginFalhas);
    }

    [Fact]
    public void LoginBemSucedido_ZeraTentativasERegistraHorario()
    {
        var usuario = Fabrica.Cidadao();
        usuario.RegistrarFalhaDeLogin(Fabrica.Agora);
        usuario.RegistrarFalhaDeLogin(Fabrica.Agora);

        usuario.RegistrarLoginBemSucedido(Fabrica.Agora);

        Assert.Equal(0, usuario.TentativasLoginFalhas);
        Assert.Null(usuario.BloqueadoAte);
        Assert.Equal(Fabrica.Agora, usuario.UltimoLoginEm);
    }
}

public class CidadaoTests
{
    [Fact]
    public void SemInteresses_LancaValidacao() =>
        Assert.Throws<ValidacaoException>(() => new Cidadao("Ana", Fabrica.Email(), "hash", Fabrica.Endereco(),
            new DateTime(2000, 1, 1), Escolaridade.MedioCompleto, []));

    [Fact]
    public void InteressesNulos_LancaValidacao() =>
        Assert.Throws<ValidacaoException>(() => new Cidadao("Ana", Fabrica.Email(), "hash", Fabrica.Endereco(),
            new DateTime(2000, 1, 1), Escolaridade.MedioCompleto, null!));

    [Fact]
    public void SemEndereco_LancaValidacao() =>
        Assert.Throws<ValidacaoException>(() => new Cidadao("Ana", Fabrica.Email(), "hash", null!,
            new DateTime(2000, 1, 1), Escolaridade.MedioCompleto, [AreaConhecimento.Artes]));

    [Fact]
    public void InteressesDuplicados_SaoRemovidos()
    {
        var cidadao = Fabrica.Cidadao(1, "Centro", AreaConhecimento.Idiomas, AreaConhecimento.Idiomas, AreaConhecimento.Artes);

        Assert.Equal(2, cidadao.AreasInteresse.Count);
        Assert.True(cidadao.TemInteresseEm(AreaConhecimento.Artes));
        Assert.False(cidadao.TemInteresseEm(AreaConhecimento.Saude));
    }

    [Theory]
    [InlineData(1800, 1, 1)]
    [InlineData(2999, 1, 1)]
    public void DataNascimentoInvalida_LancaValidacao(int ano, int mes, int dia) =>
        Assert.Throws<ValidacaoException>(() => new Cidadao("Ana", Fabrica.Email(), "hash", Fabrica.Endereco(),
            new DateTime(ano, mes, dia), Escolaridade.MedioCompleto, [AreaConhecimento.Artes]));

    [Theory]
    [InlineData(2026, 6, 14, 25)] // véspera do aniversário
    [InlineData(2026, 6, 15, 26)] // no dia do aniversário
    [InlineData(2026, 12, 31, 26)] // depois do aniversário
    public void IdadeEm_ConsideraSeJaFezAniversario(int ano, int mes, int dia, int idadeEsperada)
    {
        var cidadao = new Cidadao("Ana", Fabrica.Email(), "hash", Fabrica.Endereco(),
            new DateTime(2000, 6, 15), Escolaridade.MedioCompleto, [AreaConhecimento.Artes]);

        Assert.Equal(idadeEsperada, cidadao.IdadeEm(new DateTime(ano, mes, dia)));
    }

    [Fact]
    public void AtualizarPerfil_ParcialEValidado()
    {
        var cidadao = Fabrica.Cidadao();

        cidadao.AtualizarPerfil(null, null);
        Assert.Equal(Escolaridade.MedioCompleto, cidadao.Escolaridade);

        cidadao.AtualizarPerfil(Escolaridade.Tecnico, [AreaConhecimento.Logistica]);
        Assert.Equal(Escolaridade.Tecnico, cidadao.Escolaridade);
        Assert.Equal([AreaConhecimento.Logistica], cidadao.AreasInteresse);

        Assert.Throws<ValidacaoException>(() => cidadao.AtualizarPerfil(null, []));
    }
}

public class OrganizacaoTests
{
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,<script>")]
    [InlineData("ftp://site.com")]
    [InlineData("site sem esquema")]
    public void SiteComEsquemaPerigosoOuInvalido_LancaValidacao(string site) =>
        Assert.Throws<ValidacaoException>(() => new Organizacao("ONG", Fabrica.Email(), "hash", Fabrica.Endereco(),
            TipoOrganizacao.Ong, "Descrição", site));

    [Theory]
    [InlineData(null, null)]
    [InlineData("  ", null)]
    [InlineData("https://ong.org", "https://ong.org")]
    [InlineData("http://ong.org/sobre", "http://ong.org/sobre")]
    public void SiteValidoOuVazio_EAceito(string? site, string? esperado)
    {
        var organizacao = new Organizacao("ONG", Fabrica.Email(), "hash", Fabrica.Endereco(), TipoOrganizacao.Ong, "Desc", site);
        Assert.Equal(esperado, organizacao.Site);
    }

    [Fact]
    public void SemDescricao_LancaValidacao() =>
        Assert.Throws<ValidacaoException>(() => new Organizacao("ONG", Fabrica.Email(), "hash", Fabrica.Endereco(),
            TipoOrganizacao.Ong, "", null));

    [Fact]
    public void AtualizarApresentacao_ParcialELimpaSite()
    {
        var organizacao = Fabrica.Organizacao();

        organizacao.AtualizarApresentacao(null, null);
        Assert.Equal("Organização de teste", organizacao.Descricao);
        Assert.Equal("https://ong.teste.local", organizacao.Site);

        organizacao.AtualizarApresentacao("Nova descrição", "");
        Assert.Equal("Nova descrição", organizacao.Descricao);
        Assert.Null(organizacao.Site);

        Assert.Throws<ValidacaoException>(() => organizacao.AtualizarApresentacao(null, "javascript:x"));
    }
}
