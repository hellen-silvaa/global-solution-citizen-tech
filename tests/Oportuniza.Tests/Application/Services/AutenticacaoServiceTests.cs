using Moq;
using Oportuniza.Application.Dtos.Autenticacao;
using Oportuniza.Application.Services;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Application.Services;

public class AutenticacaoServiceTests : CenarioDeServico
{
    private readonly Cidadao _cidadao = Fabrica.Cidadao(10);
    private readonly DadosToken _token = new("jwt", "Bearer", Fabrica.Agora, 10, "Cidadã Teste", Perfil.Cidadao);

    public AutenticacaoServiceTests()
    {
        Usuarios.Setup(r => r.ObterPorEmailAsync(_cidadao.Email, It.IsAny<CancellationToken>())).ReturnsAsync(_cidadao);
        Hasher.Setup(h => h.Verificar("Senha@123", "hash")).Returns(true);
        Tokens.Setup(t => t.Gerar(_cidadao)).Returns(_token);
    }

    private AutenticacaoService Servico() => new(Usuarios.Object, Hasher.Object, Tokens.Object, UnitOfWork.Object, Relogio);

    private Task<DadosToken> Login(string senha, string? email = null) =>
        Servico().LoginAsync(new DadosLogin(email ?? _cidadao.Email.Valor, senha));

    [Fact]
    public async Task CredenciaisCorretas_GeraTokenERegistraLogin()
    {
        var resultado = await Login("Senha@123", "  CIDADAO10@teste.local ");

        Assert.Same(_token, resultado);
        Assert.Equal(Fabrica.Agora, _cidadao.UltimoLoginEm);
        Salvou();
    }

    [Fact]
    public async Task EmailInexistente_VerificaHashFalsoParaTempoConstante()
    {
        await Assert.ThrowsAsync<CredenciaisInvalidasException>(() => Login("x", "ninguem@teste.local"));

        Hasher.Verify(h => h.Verificar("x", null), Times.Once);
        NaoSalvou();
    }

    [Fact]
    public async Task EmailMalFormatado_TratadoComoInexistente()
    {
        await Assert.ThrowsAsync<CredenciaisInvalidasException>(() => Login("x", "isso-nao-e-email"));
        Hasher.Verify(h => h.Verificar("x", null), Times.Once);
    }

    [Fact]
    public async Task SenhaErrada_ContaFalhaESalva()
    {
        await Assert.ThrowsAsync<CredenciaisInvalidasException>(() => Login("errada"));

        Assert.Equal(1, _cidadao.TentativasLoginFalhas);
        Salvou();
    }

    [Fact]
    public async Task QuintaSenhaErrada_Responde429EBloqueia()
    {
        for (var i = 0; i < Usuario.MaximoTentativasLogin - 1; i++)
            await Assert.ThrowsAsync<CredenciaisInvalidasException>(() => Login("errada"));

        await Assert.ThrowsAsync<MuitasTentativasException>(() => Login("errada"));
        Assert.True(_cidadao.EstaBloqueado(Fabrica.Agora));
    }

    [Fact]
    public async Task ContaBloqueada_RecusaMesmoComSenhaCerta_EDepoisLibera()
    {
        for (var i = 0; i < Usuario.MaximoTentativasLogin; i++)
            _cidadao.RegistrarFalhaDeLogin(Fabrica.Agora);

        await Assert.ThrowsAsync<MuitasTentativasException>(() => Login("Senha@123"));
        Hasher.Verify(h => h.Verificar(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);

        Relogio.Agora = Fabrica.Agora.Add(Usuario.DuracaoBloqueio);
        Assert.Same(_token, await Login("Senha@123"));
    }

    [Fact]
    public async Task UsuarioInativo_ComSenhaCerta_CredenciaisInvalidas()
    {
        _cidadao.Desativar();

        await Assert.ThrowsAsync<CredenciaisInvalidasException>(() => Login("Senha@123"));
        Tokens.Verify(t => t.Gerar(It.IsAny<Usuario>()), Times.Never);
    }
}
