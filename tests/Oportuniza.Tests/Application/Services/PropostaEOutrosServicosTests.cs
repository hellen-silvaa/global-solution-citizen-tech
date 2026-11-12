using Microsoft.Extensions.DependencyInjection;
using Moq;
using Oportuniza.Application;
using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Enderecos;
using Oportuniza.Application.Dtos.Indicadores;
using Oportuniza.Application.Dtos.Oportunidades;
using Oportuniza.Application.Dtos.Propostas;
using Oportuniza.Application.Inscricoes.Regras;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Application.Ports.Queries;
using Oportuniza.Application.Ports.Services;
using Oportuniza.Application.Services;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Propostas;
using Oportuniza.Domain.Recomendacoes;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Application.Services;

public class PropostaServiceTests : CenarioDeServico
{
    private PropostaService Servico() => new(Propostas.Object, Cidadaos.Object, Organizacoes.Object,
        Oportunidades.Object, UnitOfWork.Object, Relogio);

    private static DadosCriacaoOportunidade Atendimento(int diasParaInicio = 10) => new("Curso", "Descrição",
        TipoOportunidade.Curso, Modalidade.Online, AreaConhecimento.Saude, "Jardim Ângela", 10, 5,
        Fabrica.Agora.AddDays(diasParaInicio), Fabrica.Agora.AddDays(diasParaInicio + 5));

    [Fact]
    public async Task Criar_PeloCidadao()
    {
        Existe(Fabrica.Cidadao(10));

        var resultado = await Servico().CriarAsync(new DadosCriacaoProposta("Curso X", "Desc", AreaConhecimento.Artes, "Centro"), ComoCidadao(10));

        Assert.Equal(StatusProposta.Aberta, resultado.Status);
        Propostas.Verify(r => r.AdicionarAsync(It.IsAny<Proposta>(), It.IsAny<CancellationToken>()), Times.Once);
        Salvou();
    }

    [Fact]
    public async Task Criar_PorOrganizacao_AcessoNegado() =>
        await Assert.ThrowsAsync<AcessoNegadoException>(() =>
            Servico().CriarAsync(new DadosCriacaoProposta("T", "D", AreaConhecimento.Artes, "Centro"), ComoOrganizacao()));

    [Fact]
    public async Task ListarEObter()
    {
        Existe(Fabrica.Proposta(50));
        var filtro = new FiltroPropostas(Status: StatusProposta.Aberta);
        Propostas.Setup(r => r.ListarAsync(filtro, It.IsAny<Paginacao>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginaResultado<Proposta>([Fabrica.Proposta(50)], 1, 10, 1));

        Assert.Single((await Servico().ListarAsync(filtro, new Paginacao())).Itens);
        Assert.Equal(50, (await Servico().ObterAsync(50)).Id);
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => Servico().ObterAsync(51));
    }

    [Fact]
    public async Task Votar_ContabilizaESalva_ESegundoVotoFalha()
    {
        Existe(Fabrica.Proposta(50));
        Existe(Fabrica.Cidadao(11));

        Assert.Equal(1, (await Servico().VotarAsync(50, ComoCidadao(11))).TotalVotos);
        await Assert.ThrowsAsync<ValidacaoException>(() => Servico().VotarAsync(50, ComoCidadao(11)));
        Salvou(1);
    }

    [Fact]
    public async Task Votar_PorOrganizacao_AcessoNegado() =>
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().VotarAsync(50, ComoOrganizacao()));

    [Fact]
    public async Task Atender_CriaOportunidadeVinculada()
    {
        var proposta = Existe(Fabrica.Proposta(50));
        Existe(Fabrica.Organizacao(20));

        var oportunidade = await Servico().AtenderAsync(50, Atendimento(), ComoOrganizacao(20));

        Assert.Equal(StatusProposta.Atendida, proposta.Status);
        Assert.Equal(20, oportunidade.OrganizacaoId);
        Oportunidades.Verify(r => r.AdicionarAsync(It.IsAny<Oportunidade>(), It.IsAny<CancellationToken>()), Times.Once);
        Salvou();
    }

    [Fact]
    public async Task Atender_PorCidadao_AcessoNegado_RN_P3() =>
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().AtenderAsync(50, Atendimento(), ComoCidadao()));

    [Fact]
    public async Task Atender_OrganizacaoInativa_OuDataPassada_LancaValidacao()
    {
        Existe(Fabrica.Proposta(50));
        var organizacao = Existe(Fabrica.Organizacao(20));

        await Assert.ThrowsAsync<ValidacaoException>(() => Servico().AtenderAsync(50, Atendimento(-1), ComoOrganizacao(20)));
        organizacao.Desativar();
        await Assert.ThrowsAsync<ValidacaoException>(() => Servico().AtenderAsync(50, Atendimento(), ComoOrganizacao(20)));
        NaoSalvou();
    }

    [Fact]
    public async Task Arquivar_AutorOuAdmin_TerceiroNao()
    {
        var proposta = Existe(Fabrica.Proposta(50, Fabrica.Cidadao(10)));

        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().ArquivarAsync(50, ComoCidadao(11)));
        await Servico().ArquivarAsync(50, ComoCidadao(10));

        Assert.Equal(StatusProposta.Arquivada, proposta.Status);
        Salvou();
    }
}

public class IndicadorServiceTests : CenarioDeServico
{
    [Fact]
    public async Task Relatorio_CalculaTaxaEDemandaReprimida()
    {
        Indicadores.Setup(r => r.ImpactoPorBairroAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new DadosIndicadorBairro("Campo Limpo", 1, 10, 0, 0, 2, 0),
            new DadosIndicadorBairro("Capão Redondo", 2, 50, 3, 1, 0, 1)
        ]);
        Indicadores.Setup(r => r.DemandaPorAreaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new DadosIndicadorArea(AreaConhecimento.Idiomas, 1, 1, 1),
            new DadosIndicadorArea(AreaConhecimento.Saude, 0, 1, 5),
            new DadosIndicadorArea(AreaConhecimento.Artes, 3, 0, 0)
        ]);

        var relatorio = await new IndicadorService(Indicadores.Object).GerarRelatorioImpactoAsync();

        Assert.Equal(4, relatorio.TotalInscricoes);
        Assert.Equal(1, relatorio.InscricoesConcluidas);
        Assert.Equal(25.0, relatorio.TaxaConclusaoPercentual);
        Assert.Equal(1, relatorio.PropostasAtendidas);
        Assert.Equal("Capão Redondo", relatorio.PorBairro[0].Bairro);
        Assert.Equal([AreaConhecimento.Saude, AreaConhecimento.Idiomas], relatorio.DemandaReprimida.Select(a => a.Area));
    }

    [Fact]
    public async Task Relatorio_SemDados_TaxaZero()
    {
        Indicadores.Setup(r => r.ImpactoPorBairroAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        Indicadores.Setup(r => r.DemandaPorAreaAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var relatorio = await new IndicadorService(Indicadores.Object).GerarRelatorioImpactoAsync();

        Assert.Equal(0, relatorio.TaxaConclusaoPercentual);
        Assert.Empty(relatorio.PorBairro);
    }
}

public class EnderecoServiceTests : CenarioDeServico
{
    [Fact]
    public async Task ConsultarCep_NormalizaAntesDeChamarOGateway()
    {
        var endereco = new DadosEndereco("01001000", "Praça da Sé", null, "Sé", "São Paulo", "SP");
        Cep.Setup(g => g.ConsultarAsync("01001000", It.IsAny<CancellationToken>())).ReturnsAsync(endereco);

        Assert.Same(endereco, await new EnderecoService(Cep.Object).ConsultarCepAsync("01001-000"));
    }

    [Fact]
    public async Task ConsultarCep_Invalido_NaoChamaOGateway()
    {
        await Assert.ThrowsAsync<ValidacaoException>(() => new EnderecoService(Cep.Object).ConsultarCepAsync("abc"));
        Cep.Verify(g => g.ConsultarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class ApplicationDependencyInjectionTests
{
    [Fact]
    public void AddApplication_ResolveTodosOsCasosDeUso()
    {
        var services = new ServiceCollection().AddApplication();
        foreach (var porta in new[]
                 {
                     typeof(IUsuarioRepository), typeof(ICidadaoRepository), typeof(IOrganizacaoRepository),
                     typeof(IOportunidadeRepository), typeof(IInscricaoRepository), typeof(IPropostaRepository),
                     typeof(IIndicadorQueries), typeof(IUnitOfWork), typeof(IPasswordHasher), typeof(ITokenService),
                     typeof(IConsultaCepGateway)
                 })
            services.AddScoped(porta, _ => CriarMock(porta));
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IAutenticacaoService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICidadaoService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IOrganizacaoService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IOportunidadeService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IInscricaoService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IPropostaService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IIndicadorService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IEnderecoService>());
        Assert.Equal(2, scope.ServiceProvider.GetServices<IRegraInscricao>().Count());
        Assert.Equal(4, scope.ServiceProvider.GetServices<ICriterioRecomendacao>().Count());
    }

    private static object CriarMock(Type tipo) => ((Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(tipo))!).Object;
}
