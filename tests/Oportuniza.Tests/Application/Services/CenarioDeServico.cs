using Moq;
using Oportuniza.Application.Common;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Application.Ports.Queries;
using Oportuniza.Application.Ports.Services;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Inscricoes;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Propostas;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Application.Services;

/// <summary>Base dos testes de casos de uso: todas as portas simuladas com Moq e um relógio fixo.</summary>
public abstract class CenarioDeServico
{
    protected Mock<IUsuarioRepository> Usuarios { get; } = new();
    protected Mock<ICidadaoRepository> Cidadaos { get; } = new();
    protected Mock<IOrganizacaoRepository> Organizacoes { get; } = new();
    protected Mock<IOportunidadeRepository> Oportunidades { get; } = new();
    protected Mock<IInscricaoRepository> Inscricoes { get; } = new();
    protected Mock<IPropostaRepository> Propostas { get; } = new();
    protected Mock<IIndicadorQueries> Indicadores { get; } = new();
    protected Mock<IUnitOfWork> UnitOfWork { get; } = new();
    protected Mock<IPasswordHasher> Hasher { get; } = new();
    protected Mock<ITokenService> Tokens { get; } = new();
    protected Mock<IConsultaCepGateway> Cep { get; } = new();
    protected RelogioFixo Relogio { get; } = new(Fabrica.Agora);

    protected static readonly UsuarioAutenticado Admin = new(1, Perfil.Admin);
    protected static UsuarioAutenticado ComoCidadao(long id = 10) => new(id, Perfil.Cidadao);
    protected static UsuarioAutenticado ComoOrganizacao(long id = 20) => new(id, Perfil.Organizacao);

    protected CenarioDeServico() => Hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hash");

    protected Cidadao Existe(Cidadao cidadao)
    {
        Cidadaos.Setup(r => r.ObterPorIdAsync(cidadao.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cidadao);
        return cidadao;
    }

    protected Organizacao Existe(Organizacao organizacao)
    {
        Organizacoes.Setup(r => r.ObterPorIdAsync(organizacao.Id, It.IsAny<CancellationToken>())).ReturnsAsync(organizacao);
        return organizacao;
    }

    protected Oportunidade Existe(Oportunidade oportunidade)
    {
        Oportunidades.Setup(r => r.ObterPorIdAsync(oportunidade.Id, It.IsAny<CancellationToken>())).ReturnsAsync(oportunidade);
        return oportunidade;
    }

    protected Inscricao Existe(Inscricao inscricao)
    {
        Inscricoes.Setup(r => r.ObterPorIdAsync(inscricao.Id, It.IsAny<CancellationToken>())).ReturnsAsync(inscricao);
        return inscricao;
    }

    protected Proposta Existe(Proposta proposta)
    {
        Propostas.Setup(r => r.ObterPorIdAsync(proposta.Id, It.IsAny<CancellationToken>())).ReturnsAsync(proposta);
        return proposta;
    }

    protected void Salvou(int vezes = 1) =>
        UnitOfWork.Verify(u => u.SalvarAsync(It.IsAny<CancellationToken>()), Times.Exactly(vezes));

    protected void NaoSalvou() => Salvou(0);
}
