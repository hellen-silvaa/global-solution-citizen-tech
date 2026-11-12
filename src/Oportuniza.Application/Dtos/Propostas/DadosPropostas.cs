using System.ComponentModel.DataAnnotations;
using Oportuniza.Domain.Propostas;
using Oportuniza.Domain.Enums;

namespace Oportuniza.Application.Dtos.Propostas;

/// <summary>Proposta de qualificação que falta no bairro (participação cidadã).</summary>
/// <param name="Titulo">Título (até 120 caracteres).</param>
/// <param name="Descricao">Por que a qualificação é importante (até 1000 caracteres).</param>
/// <param name="Area">Área de conhecimento.</param>
/// <param name="Bairro">Bairro onde falta a qualificação.</param>
public record DadosCriacaoProposta(
    [Required(ErrorMessage = "Título é obrigatório")] [StringLength(120)] string Titulo,
    [Required(ErrorMessage = "Descrição é obrigatória")] [StringLength(1000)] string Descricao,
    [Required(ErrorMessage = "Área é obrigatória")] AreaConhecimento Area,
    [Required(ErrorMessage = "Bairro é obrigatório")] [StringLength(80)] string Bairro);

/// <summary>Proposta com a contagem de votos.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Titulo">Título.</param>
/// <param name="Descricao">Descrição.</param>
/// <param name="Area">Área.</param>
/// <param name="Bairro">Bairro.</param>
/// <param name="Status">Aberta, Atendida ou Arquivada.</param>
/// <param name="TotalVotos">Votos recebidos (cada cidadão vota uma única vez).</param>
/// <param name="AutorId">Cidadão autor.</param>
/// <param name="AutorNome">Nome do autor.</param>
/// <param name="OportunidadeGeradaId">Oportunidade criada para atender a proposta, se já atendida.</param>
/// <param name="CriadaEm">Data de criação (UTC).</param>
public record DadosDetalhamentoProposta(
    long Id, string Titulo, string Descricao, AreaConhecimento Area, string Bairro, StatusProposta Status,
    int TotalVotos, long AutorId, string? AutorNome, long? OportunidadeGeradaId, DateTime CriadaEm)
{
    public static DadosDetalhamentoProposta De(Proposta p) => new(
        p.Id, p.Titulo, p.Descricao, p.Area, p.Bairro, p.Status, p.TotalVotos,
        p.AutorId, p.Autor?.Nome, p.OportunidadeGeradaId ?? p.OportunidadeGerada?.Id, p.CriadoEm);
}
