using Oportuniza.Domain.Enums;

namespace Oportuniza.Application.Dtos.Indicadores;

/// <summary>Indicador de impacto por bairro: oferta, procura e participação.</summary>
/// <param name="Bairro">Bairro.</param>
/// <param name="OportunidadesAbertas">Oportunidades abertas no bairro.</param>
/// <param name="VagasOfertadas">Total de vagas nessas oportunidades.</param>
/// <param name="InscricoesAtivas">Inscrições em andamento.</param>
/// <param name="InscricoesConcluidas">Inscrições concluídas.</param>
/// <param name="PropostasAbertas">Propostas ainda em votação.</param>
/// <param name="PropostasAtendidas">Propostas já atendidas por uma organização.</param>
public record DadosIndicadorBairro(
    string Bairro,
    int OportunidadesAbertas,
    int VagasOfertadas,
    int InscricoesAtivas,
    int InscricoesConcluidas,
    int PropostasAbertas,
    int PropostasAtendidas);

/// <summary>Oferta x demanda por área de conhecimento.</summary>
/// <param name="Area">Área de conhecimento.</param>
/// <param name="OportunidadesAbertas">Oferta: oportunidades abertas na área.</param>
/// <param name="PropostasAbertas">Procura: propostas abertas na área.</param>
/// <param name="VotosEmPropostasAbertas">Votos acumulados nessas propostas (demanda reprimida).</param>
public record DadosIndicadorArea(
    AreaConhecimento Area,
    int OportunidadesAbertas,
    int PropostasAbertas,
    int VotosEmPropostasAbertas);

/// <summary>Relatório consolidado: é o "indicador de impacto" da Ficha da CitizenTech.</summary>
/// <param name="TotalInscricoes">Total de inscrições (exceto canceladas).</param>
/// <param name="InscricoesConcluidas">Inscrições que viraram qualificação concluída.</param>
/// <param name="TaxaConclusaoPercentual">Concluídas ÷ total de inscrições, em %.</param>
/// <param name="PropostasAtendidas">Propostas da comunidade atendidas.</param>
/// <param name="PorBairro">Oferta, procura e participação por bairro.</param>
/// <param name="DemandaReprimida">Áreas em que a comunidade pediu e ainda falta oferta.</param>
public record DadosRelatorioImpacto(
    int TotalInscricoes,
    int InscricoesConcluidas,
    double TaxaConclusaoPercentual,
    int PropostasAtendidas,
    IReadOnlyList<DadosIndicadorBairro> PorBairro,
    IReadOnlyList<DadosIndicadorArea> DemandaReprimida);
