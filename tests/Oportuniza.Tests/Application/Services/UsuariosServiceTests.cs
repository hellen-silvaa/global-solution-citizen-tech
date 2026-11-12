using Moq;
using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Enderecos;
using Oportuniza.Application.Dtos.Usuarios;
using Oportuniza.Application.Services;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Application.Services;

public class CidadaoServiceTests : CenarioDeServico
{
    private static readonly DadosEndereco Endereco = new("05863-000", "Rua", "1", "Centro", "São Paulo", "SP");

    private CidadaoService Servico() =>
        new(Cidadaos.Object, Usuarios.Object, Inscricoes.Object, Hasher.Object, UnitOfWork.Object);

    private static DadosCadastroCidadao Cadastro(string email = "nova@teste.local") =>
        new("Nova", email, "Senha@123", new DateTime(2001, 5, 5), Escolaridade.MedioCompleto, [AreaConhecimento.Artes], Endereco);

    [Fact]
    public async Task Cadastrar_GuardaHashDaSenhaESalva()
    {
        Cidadao? adicionado = null;
        Cidadaos.Setup(r => r.AdicionarAsync(It.IsAny<Cidadao>(), It.IsAny<CancellationToken>()))
            .Callback<Cidadao, CancellationToken>((c, _) => adicionado = c);

        var resultado = await Servico().CadastrarAsync(Cadastro("  NOVA@teste.local"));

        Assert.Equal("nova@teste.local", resultado.Email);
        Assert.Equal("hash", adicionado!.SenhaHash);
        Hasher.Verify(h => h.Hash("Senha@123"), Times.Once);
        Salvou();
    }

    [Fact]
    public async Task Cadastrar_EmailEmUso_LancaValidacao()
    {
        Usuarios.Setup(r => r.EmailEmUsoAsync(Email.Criar("nova@teste.local"), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ValidacaoException>(() => Servico().CadastrarAsync(Cadastro()));
        NaoSalvou();
    }

    [Fact]
    public async Task Obter_ProprioOuAdmin_OutroCidadaoNao()
    {
        Existe(Fabrica.Cidadao(10));

        Assert.Equal(10, (await Servico().ObterAsync(10, ComoCidadao(10))).Id);
        Assert.Equal(10, (await Servico().ObterAsync(10, Admin)).Id);
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().ObterAsync(10, ComoCidadao(11)));
    }

    [Fact]
    public async Task Obter_Inexistente_404() =>
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => Servico().ObterAsync(10, Admin));

    [Fact]
    public async Task Listar_MapeiaParaListagem()
    {
        Cidadaos.Setup(r => r.ListarAsync("Centro", It.IsAny<Paginacao>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginaResultado<Cidadao>([Fabrica.Cidadao(10, "Centro")], 1, 10, 1));

        var pagina = await Servico().ListarAsync("Centro", new Paginacao());

        Assert.Equal("Centro", Assert.Single(pagina.Itens).Bairro);
    }

    [Fact]
    public async Task Atualizar_AplicaMudancasESalva()
    {
        var cidadao = Existe(Fabrica.Cidadao(10));

        var resultado = await Servico().AtualizarAsync(10,
            new DadosAtualizacaoCidadao("Novo", Escolaridade.Tecnico, [AreaConhecimento.Idiomas], Endereco), ComoCidadao(10));

        Assert.Equal("Novo", resultado.Nome);
        Assert.Equal("Centro", cidadao.Endereco!.Bairro);
        Assert.Equal(Escolaridade.Tecnico, cidadao.Escolaridade);
        Salvou();
    }

    [Fact]
    public async Task Atualizar_OutroCidadao_AcessoNegadoSemSalvar()
    {
        Existe(Fabrica.Cidadao(10));
        await Assert.ThrowsAsync<AcessoNegadoException>(() =>
            Servico().AtualizarAsync(10, new DadosAtualizacaoCidadao(null, null, null, null), ComoCidadao(11)));
        NaoSalvou();
    }

    [Fact]
    public async Task Desativar_ExclusaoLogica()
    {
        var cidadao = Existe(Fabrica.Cidadao(10));

        await Servico().DesativarAsync(10, ComoCidadao(10));

        Assert.False(cidadao.Ativo);
        Salvou();
    }

    [Fact]
    public async Task ListarInscricoes_FiltraPorStatus()
    {
        Existe(Fabrica.Cidadao(10));
        Inscricoes.Setup(r => r.ListarPorCidadaoAsync(10, StatusInscricao.Pendente, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Fabrica.Inscricao()]);

        var lista = await Servico().ListarInscricoesAsync(10, StatusInscricao.Pendente, ComoCidadao(10));

        Assert.Single(lista);
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().ListarInscricoesAsync(10, null, ComoCidadao(11)));
    }
}

public class OrganizacaoServiceTests : CenarioDeServico
{
    private static readonly DadosEndereco Endereco = new("05863-000", "Rua", "1", "Centro", "São Paulo", "SP");

    private OrganizacaoService Servico() => new(Organizacoes.Object, Usuarios.Object, Hasher.Object, UnitOfWork.Object);

    private static DadosCadastroOrganizacao Cadastro() =>
        new("ONG", "ong@teste.local", "Senha@123", TipoOrganizacao.Ong, "Descrição", "https://ong.org", Endereco);

    [Fact]
    public async Task Cadastrar_Salva()
    {
        var resultado = await Servico().CadastrarAsync(Cadastro());

        Assert.Equal(TipoOrganizacao.Ong, resultado.Tipo);
        Organizacoes.Verify(r => r.AdicionarAsync(It.IsAny<Organizacao>(), It.IsAny<CancellationToken>()), Times.Once);
        Salvou();
    }

    [Fact]
    public async Task Cadastrar_EmailEmUso_LancaValidacao()
    {
        Usuarios.Setup(r => r.EmailEmUsoAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        await Assert.ThrowsAsync<ValidacaoException>(() => Servico().CadastrarAsync(Cadastro()));
    }

    [Fact]
    public async Task ObterEListar()
    {
        Existe(Fabrica.Organizacao(20));
        Organizacoes.Setup(r => r.ListarAsync(TipoOrganizacao.Ong, It.IsAny<Paginacao>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginaResultado<Organizacao>([Fabrica.Organizacao(20)], 1, 10, 1));

        Assert.Equal(20, (await Servico().ObterAsync(20)).Id);
        Assert.Single((await Servico().ListarAsync(TipoOrganizacao.Ong, new Paginacao())).Itens);
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => Servico().ObterAsync(21));
    }

    [Fact]
    public async Task Atualizar_PropriaOrganizacao()
    {
        var organizacao = Existe(Fabrica.Organizacao(20));

        await Servico().AtualizarAsync(20, new DadosAtualizacaoOrganizacao("Novo nome", "Nova desc", "", Endereco), ComoOrganizacao(20));

        Assert.Equal("Novo nome", organizacao.Nome);
        Assert.Null(organizacao.Site);
        Salvou();
    }

    [Fact]
    public async Task AtualizarEDesativar_OutraOrganizacao_AcessoNegado()
    {
        Existe(Fabrica.Organizacao(20));

        await Assert.ThrowsAsync<AcessoNegadoException>(() =>
            Servico().AtualizarAsync(20, new DadosAtualizacaoOrganizacao(null, null, null, null), ComoOrganizacao(21)));
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().DesativarAsync(20, ComoOrganizacao(21)));
        NaoSalvou();
    }

    [Fact]
    public async Task Desativar_PeloAdmin()
    {
        var organizacao = Existe(Fabrica.Organizacao(20));
        await Servico().DesativarAsync(20, Admin);
        Assert.False(organizacao.Ativo);
    }
}
