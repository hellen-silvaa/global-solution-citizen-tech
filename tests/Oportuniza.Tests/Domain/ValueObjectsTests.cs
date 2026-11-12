using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.ValueObjects;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Domain;

public class EmailTests
{
    [Fact]
    public void Criar_NormalizaEspacosEMaiusculas()
    {
        var email = Email.Criar("  Ana@Exemplo.LOCAL ");

        Assert.Equal("ana@exemplo.local", email.Valor);
        Assert.Equal("ana@exemplo.local", email.ToString());
    }

    [Fact]
    public void Igualdade_EPorValor()
    {
        Assert.Equal(Email.Criar("ana@x.com"), Email.Criar("ANA@x.com"));
        Assert.NotEqual(Email.Criar("ana@x.com"), Email.Criar("bia@x.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sem-arroba")]
    [InlineData("a@b")]
    [InlineData("a b@c.com")]
    [InlineData("a@@c.com")]
    public void Criar_FormatoInvalido_LancaValidacao(string? valor) =>
        Assert.Throws<ValidacaoException>(() => Email.Criar(valor));

    [Fact]
    public void Criar_MuitoLongo_LancaValidacao() =>
        Assert.Throws<ValidacaoException>(() => Email.Criar(new string('a', 115) + "@x.com"));
}

public class EnderecoTests
{
    [Theory]
    [InlineData("05863-000", "05863000")]
    [InlineData(" 01001000 ", "01001000")]
    public void NormalizarCep_RemoveHifenEEspacos(string entrada, string esperado) =>
        Assert.Equal(esperado, Endereco.NormalizarCep(entrada));

    [Theory]
    [InlineData(null)]
    [InlineData("123")]
    [InlineData("abcde-fgh")]
    [InlineData("0586300")]
    [InlineData("05863-0000")]
    public void NormalizarCep_Invalido_LancaValidacao(string? cep) =>
        Assert.Throws<ValidacaoException>(() => Endereco.NormalizarCep(cep));

    [Fact]
    public void Construtor_NormalizaUfENumeroVazio()
    {
        var endereco = new Endereco("05863-000", " Rua A ", "  ", "Centro", "São Paulo", "sp");

        Assert.Equal("05863000", endereco.Cep);
        Assert.Equal("Rua A", endereco.Logradouro);
        Assert.Null(endereco.Numero);
        Assert.Equal("SP", endereco.Uf);
    }

    [Theory]
    [InlineData("S")]
    [InlineData("S1")]
    [InlineData("SPA")]
    public void Construtor_UfInvalida_LancaValidacao(string uf) =>
        Assert.Throws<ValidacaoException>(() => new Endereco("05863000", "Rua", "1", "Centro", "SP", uf));

    [Theory]
    [InlineData("", "Centro", "São Paulo")]
    [InlineData("Rua", "", "São Paulo")]
    [InlineData("Rua", "Centro", "")]
    public void Construtor_CampoObrigatorioVazio_LancaValidacao(string logradouro, string bairro, string cidade) =>
        Assert.Throws<ValidacaoException>(() => new Endereco("05863000", logradouro, null, bairro, cidade, "SP"));

    [Fact]
    public void Construtor_NumeroMuitoLongo_LancaValidacao() =>
        Assert.Throws<ValidacaoException>(() => new Endereco("05863000", "Rua", "12345678901", "Centro", "SP", "SP"));

    [Theory]
    [InlineData("capão redondo", true)]
    [InlineData(" Capão Redondo ", true)]
    [InlineData("Campo Limpo", false)]
    [InlineData(null, false)]
    public void MesmoBairro_IgnoraMaiusculasEEspacos(string? bairro, bool esperado) =>
        Assert.Equal(esperado, Fabrica.Endereco("Capão Redondo").MesmoBairro(bairro));
}

public class PeriodoTests
{
    [Fact]
    public void Construtor_FimAntesDoInicio_LancaValidacao() =>
        Assert.Throws<ValidacaoException>(() => new Periodo(Fabrica.Agora, Fabrica.Agora.AddDays(-1)));

    [Fact]
    public void Construtor_MesmoDia_EValido()
    {
        var periodo = new Periodo(Fabrica.Agora, Fabrica.Agora);
        Assert.Equal(periodo.Inicio, periodo.Fim);
    }

    [Fact]
    public void JaComecouEm_ComparaComOInicio()
    {
        var periodo = new Periodo(Fabrica.Agora, Fabrica.Agora.AddDays(1));

        Assert.False(periodo.JaComecouEm(Fabrica.Agora.AddSeconds(-1)));
        Assert.True(periodo.JaComecouEm(Fabrica.Agora));
        Assert.True(periodo.JaComecouEm(Fabrica.Agora.AddDays(2)));
    }
}
