using Oportuniza.Application.Dtos.Oportunidades;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Application.Services;

/// <summary>
/// Cria oportunidades a partir do DTO. Compartilhada por quem publica direto (OportunidadeService)
/// e por quem atende uma proposta (PropostaService), para que as regras de criação fiquem em um só lugar.
/// </summary>
internal static class FabricaDeOportunidades
{
    public static Oportunidade Criar(Organizacao organizacao, DadosCriacaoOportunidade dados, DateTime agora)
    {
        if (!organizacao.Ativo)
            throw new ValidacaoException("Organização inativa não pode publicar oportunidades!");
        if (dados.DataInicio <= agora)
            throw new ValidacaoException("Data de início deve ser futura!");

        return new Oportunidade(organizacao, dados.Titulo, dados.Descricao, dados.Tipo, dados.Modalidade,
            dados.Area, dados.Bairro, dados.CargaHoraria, dados.Vagas, new Periodo(dados.DataInicio, dados.DataFim),
            dados.Gratuita);
    }
}
