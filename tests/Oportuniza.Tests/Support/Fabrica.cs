using Oportuniza.Domain.Common;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Inscricoes;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Propostas;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Tests.Support;

/// <summary>Monta objetos de domínio válidos para os testes (sem banco).</summary>
public static class Fabrica
{
    public static readonly DateTime Agora = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);

    public static Email Email(string valor = "teste@teste.local") => global::Oportuniza.Domain.ValueObjects.Email.Criar(valor);

    public static Endereco Endereco(string bairro = "Capão Redondo") =>
        new("05863-000", "Rua das Flores", "45", bairro, "São Paulo", "SP");

    public static Periodo Periodo(int diasParaInicio = 10, int duracaoDias = 30) =>
        new(Agora.AddDays(diasParaInicio), Agora.AddDays(diasParaInicio + duracaoDias));

    public static Cidadao Cidadao(long id = 10, string bairro = "Capão Redondo", params AreaConhecimento[] interesses) =>
        ComId(new Cidadao("Cidadã Teste", Email($"cidadao{id}@teste.local"), "hash", Endereco(bairro),
            new DateTime(2000, 1, 1), Escolaridade.MedioCompleto,
            interesses.Length == 0 ? [AreaConhecimento.Tecnologia] : interesses), id);

    public static Organizacao Organizacao(long id = 20) =>
        ComId(new Organizacao("ONG Teste", Email($"org{id}@teste.local"), "hash", Endereco(),
            TipoOrganizacao.Ong, "Organização de teste", "https://ong.teste.local"), id);

    public static Administrador Admin(long id = 1) =>
        ComId(new Administrador("Admin", Email("admin@teste.local"), "hash"), id);

    public static Oportunidade Oportunidade(
        long id = 30, Organizacao? organizacao = null, int vagas = 10,
        AreaConhecimento area = AreaConhecimento.Tecnologia, string bairro = "Capão Redondo",
        Modalidade modalidade = Modalidade.Presencial, int diasParaInicio = 10, bool gratuita = true) =>
        ComId(new Oportunidade(organizacao ?? Organizacao(), "Curso de teste", "Descrição", TipoOportunidade.Curso,
            modalidade, area, bairro, 20, vagas, Periodo(diasParaInicio), gratuita), id);

    public static Inscricao Inscricao(long id = 40, Cidadao? cidadao = null, Oportunidade? oportunidade = null) =>
        ComId((oportunidade ?? Oportunidade()).Inscrever(cidadao ?? Cidadao(), Agora), id);

    public static Proposta Proposta(long id = 50, Cidadao? autor = null) =>
        ComId(new Proposta(autor ?? Cidadao(), "Curso de cuidador", "Falta no bairro", AreaConhecimento.Saude, "Jardim Ângela"), id);

    /// <summary>Define o Id (setter protegido) como o EF Core faria ao salvar.</summary>
    public static T ComId<T>(T entidade, long id) where T : Entidade
    {
        typeof(Entidade).GetProperty(nameof(Entidade.Id))!.SetValue(entidade, id);
        return entidade;
    }
}
