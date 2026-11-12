using Moq;
using Oportuniza.Application.Inscricoes.Regras;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Application;

public class RegrasInscricaoTests
{
    private readonly Mock<IInscricaoRepository> _inscricoes = new();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RN_I3_InscricaoUnica(bool jaInscrito)
    {
        _inscricoes.Setup(r => r.ExisteInscricaoAtivaAsync(10, 30, It.IsAny<CancellationToken>())).ReturnsAsync(jaInscrito);
        var regra = new RegraInscricaoUnica(_inscricoes.Object);

        var validar = () => regra.ValidarAsync(Fabrica.Cidadao(10), Fabrica.Oportunidade(30));

        if (jaInscrito)
            await Assert.ThrowsAsync<ValidacaoException>(validar);
        else
            await validar();
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(RegraLimiteInscricoesAtivas.Limite - 1, false)]
    [InlineData(RegraLimiteInscricoesAtivas.Limite, true)]
    [InlineData(RegraLimiteInscricoesAtivas.Limite + 1, true)]
    public async Task RN_I7_LimiteDeInscricoesAtivas(int ativas, bool deveBloquear)
    {
        _inscricoes.Setup(r => r.ContarAtivasDoCidadaoAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(ativas);
        var regra = new RegraLimiteInscricoesAtivas(_inscricoes.Object);

        var validar = () => regra.ValidarAsync(Fabrica.Cidadao(10), Fabrica.Oportunidade());

        if (deveBloquear)
            Assert.Contains("Limite", (await Assert.ThrowsAsync<ValidacaoException>(validar)).Message);
        else
            await validar();
    }
}
