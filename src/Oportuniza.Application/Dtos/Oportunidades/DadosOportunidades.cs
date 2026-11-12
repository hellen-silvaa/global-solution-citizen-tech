using System.ComponentModel.DataAnnotations;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Recomendacoes;

namespace Oportuniza.Application.Dtos.Oportunidades;

/// <summary>Dados para a organização publicar uma oportunidade.</summary>
/// <param name="Titulo">Título (até 120 caracteres).</param>
/// <param name="Descricao">O que será ensinado e para quem (até 1000 caracteres).</param>
/// <param name="Tipo">Curso, Oficina, Mentoria ou JovemAprendiz.</param>
/// <param name="Modalidade">Presencial, Online ou Hibrida.</param>
/// <param name="Area">Área de conhecimento.</param>
/// <param name="Bairro">Bairro onde acontece.</param>
/// <param name="CargaHoraria">Carga horária total, de 1 a 2000 horas.</param>
/// <param name="Vagas">Quantidade de vagas, de 1 a 1000.</param>
/// <param name="DataInicio">Data de início.</param>
/// <param name="DataFim">Data de término (posterior ao início).</param>
/// <param name="Gratuita">Se é gratuita. Padrão: true.</param>
public record DadosCriacaoOportunidade(
    [Required(ErrorMessage = "Título é obrigatório")] [StringLength(120)] string Titulo,
    [Required(ErrorMessage = "Descrição é obrigatória")] [StringLength(1000)] string Descricao,
    [Required(ErrorMessage = "Tipo é obrigatório")] TipoOportunidade Tipo,
    [Required(ErrorMessage = "Modalidade é obrigatória")] Modalidade Modalidade,
    [Required(ErrorMessage = "Área é obrigatória")] AreaConhecimento Area,
    [Required(ErrorMessage = "Bairro é obrigatório")] [StringLength(80)] string Bairro,
    [Range(1, 2000, ErrorMessage = "Carga horária deve ser entre 1 e 2000 horas")] int CargaHoraria,
    [Range(1, 1000, ErrorMessage = "Vagas deve ser entre 1 e 1000")] int Vagas,
    [Required(ErrorMessage = "Data de início é obrigatória")] DateTime DataInicio,
    [Required(ErrorMessage = "Data de término é obrigatória")] DateTime DataFim,
    bool Gratuita = true);

/// <summary>Dados para a organização dona alterar uma oportunidade ainda aberta.</summary>
/// <param name="Titulo">Título (até 120 caracteres).</param>
/// <param name="Descricao">Descrição (até 1000 caracteres).</param>
/// <param name="Modalidade">Presencial, Online ou Hibrida.</param>
/// <param name="Area">Área de conhecimento.</param>
/// <param name="Bairro">Bairro onde acontece.</param>
/// <param name="CargaHoraria">Carga horária, de 1 a 2000 horas.</param>
/// <param name="Vagas">Vagas, de 1 a 1000.</param>
/// <param name="DataInicio">Data de início.</param>
/// <param name="DataFim">Data de término.</param>
public record DadosAtualizacaoOportunidade(
    [Required(ErrorMessage = "Título é obrigatório")] [StringLength(120)] string Titulo,
    [Required(ErrorMessage = "Descrição é obrigatória")] [StringLength(1000)] string Descricao,
    [Required] Modalidade Modalidade,
    [Required] AreaConhecimento Area,
    [Required(ErrorMessage = "Bairro é obrigatório")] [StringLength(80)] string Bairro,
    [Range(1, 2000, ErrorMessage = "Carga horária deve ser entre 1 e 2000 horas")] int CargaHoraria,
    [Range(1, 1000, ErrorMessage = "Vagas deve ser entre 1 e 1000")] int Vagas,
    [Required] DateTime DataInicio,
    [Required] DateTime DataFim);

/// <summary>Oportunidade com as vagas restantes e a organização dona.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Titulo">Título.</param>
/// <param name="Descricao">Descrição.</param>
/// <param name="Tipo">Tipo da oportunidade.</param>
/// <param name="Modalidade">Modalidade.</param>
/// <param name="Area">Área de conhecimento.</param>
/// <param name="Bairro">Bairro.</param>
/// <param name="CargaHoraria">Carga horária em horas.</param>
/// <param name="VagasTotais">Total de vagas ofertadas.</param>
/// <param name="VagasDisponiveis">Vagas ainda livres (totais menos inscrições ativas).</param>
/// <param name="DataInicio">Início.</param>
/// <param name="DataFim">Término.</param>
/// <param name="Gratuita">Se é gratuita.</param>
/// <param name="Status">Aberta, Encerrada ou Cancelada.</param>
/// <param name="OrganizacaoId">Identificador da organização dona.</param>
/// <param name="OrganizacaoNome">Nome da organização dona.</param>
public record DadosDetalhamentoOportunidade(
    long Id, string Titulo, string Descricao, TipoOportunidade Tipo, Modalidade Modalidade,
    AreaConhecimento Area, string Bairro, int CargaHoraria, int VagasTotais, int VagasDisponiveis,
    DateTime DataInicio, DateTime DataFim, bool Gratuita, StatusOportunidade Status,
    long OrganizacaoId, string? OrganizacaoNome)
{
    public static DadosDetalhamentoOportunidade De(Oportunidade o) => new(
        o.Id, o.Titulo, o.Descricao, o.Tipo, o.Modalidade, o.Area, o.Bairro, o.CargaHoraria,
        o.VagasTotais, o.VagasDisponiveis, o.Periodo.Inicio, o.Periodo.Fim, o.Gratuita, o.Status,
        o.OrganizacaoId, o.Organizacao?.Nome);
}

/// <summary>Oportunidade recomendada a um cidadão, com a pontuação e os motivos da recomendação.</summary>
/// <param name="Oportunidade">Oportunidade recomendada.</param>
/// <param name="Pontuacao">Pontuação somada pelos critérios (quanto maior, mais aderente).</param>
/// <param name="Motivos">Explicação de cada critério que pontuou (recomendação transparente, sem caixa-preta).</param>
public record DadosRecomendacao(DadosDetalhamentoOportunidade Oportunidade, int Pontuacao, IReadOnlyList<string> Motivos)
{
    public static DadosRecomendacao De(Recomendacao r) => new(
        DadosDetalhamentoOportunidade.De(r.Oportunidade), r.Pontuacao, r.Motivos.Select(m => m.Descricao).ToList());
}
