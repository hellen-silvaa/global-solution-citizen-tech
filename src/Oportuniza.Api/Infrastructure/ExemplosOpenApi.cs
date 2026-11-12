using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Oportuniza.Application.Dtos.Autenticacao;
using Oportuniza.Application.Dtos.Enderecos;
using Oportuniza.Application.Dtos.Indicadores;
using Oportuniza.Application.Dtos.Inscricoes;
using Oportuniza.Application.Dtos.Oportunidades;
using Oportuniza.Application.Dtos.Propostas;
using Oportuniza.Application.Dtos.Usuarios;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Oportuniza.Api.Infrastructure;

/// <summary>
/// Exemplos de requisição e resposta exibidos no Swagger UI (aparecem pré-preenchidos no "Try it out").
/// Todos os dados são fictícios.
/// </summary>
public sealed class ExemplosSchemaFilter : ISchemaFilter
{
    private static readonly JsonObject Endereco = new()
    {
        ["cep"] = "05767-000",
        ["logradouro"] = "Estrada de Itapecerica",
        ["numero"] = "1200",
        ["bairro"] = "Capão Redondo",
        ["cidade"] = "São Paulo",
        ["uf"] = "SP"
    };

    private static JsonObject Oportunidade() => new()
    {
        ["id"] = 3,
        ["titulo"] = "Curso de Programação Web",
        ["descricao"] = "Introdução gratuita a HTML, CSS, JavaScript e C# para jovens do bairro.",
        ["tipo"] = "Curso",
        ["modalidade"] = "Presencial",
        ["area"] = "Tecnologia",
        ["bairro"] = "Capão Redondo",
        ["cargaHoraria"] = 80,
        ["vagasTotais"] = 30,
        ["vagasDisponiveis"] = 12,
        ["dataInicio"] = "2026-12-01T00:00:00",
        ["dataFim"] = "2027-02-28T00:00:00",
        ["gratuita"] = true,
        ["status"] = "Aberta",
        ["organizacaoId"] = 2,
        ["organizacaoNome"] = "Instituto Recomeçar"
    };

    private static readonly Dictionary<Type, Func<JsonNode>> Exemplos = new()
    {
        [typeof(DadosLogin)] = () => new JsonObject { ["email"] = "ana.souza@exemplo.com", ["senha"] = "Senha@123" },
        [typeof(DadosToken)] = () => new JsonObject
        {
            ["token"] = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            ["tipo"] = "Bearer",
            ["expiraEm"] = "2026-11-11T14:00:00Z",
            ["usuarioId"] = 5,
            ["nome"] = "Ana Souza",
            ["perfil"] = "Cidadao"
        },
        [typeof(DadosEndereco)] = () => Endereco.DeepClone(),
        [typeof(DadosCadastroCidadao)] = () => new JsonObject
        {
            ["nome"] = "Ana Souza",
            ["email"] = "ana.souza@exemplo.com",
            ["senha"] = "Senha@123",
            ["dataNascimento"] = "2004-06-15T00:00:00",
            ["escolaridade"] = "MedioCompleto",
            ["areasInteresse"] = new JsonArray("Tecnologia", "Idiomas"),
            ["endereco"] = Endereco.DeepClone()
        },
        [typeof(DadosCadastroOrganizacao)] = () => new JsonObject
        {
            ["nome"] = "Instituto Recomeçar",
            ["email"] = "contato@recomecar.exemplo.org",
            ["senha"] = "Senha@123",
            ["tipo"] = "Ong",
            ["descricao"] = "ONG que oferece cursos gratuitos de qualificação profissional para jovens.",
            ["site"] = "https://recomecar.exemplo.org",
            ["endereco"] = Endereco.DeepClone()
        },
        [typeof(DadosCriacaoOportunidade)] = () => new JsonObject
        {
            ["titulo"] = "Curso de Programação Web",
            ["descricao"] = "Introdução gratuita a HTML, CSS, JavaScript e C# para jovens do bairro.",
            ["tipo"] = "Curso",
            ["modalidade"] = "Presencial",
            ["area"] = "Tecnologia",
            ["bairro"] = "Capão Redondo",
            ["cargaHoraria"] = 80,
            ["vagas"] = 30,
            ["dataInicio"] = "2026-12-01T00:00:00",
            ["dataFim"] = "2027-02-28T00:00:00",
            ["gratuita"] = true
        },
        [typeof(DadosDetalhamentoOportunidade)] = () => Oportunidade(),
        [typeof(DadosRecomendacao)] = () => new JsonObject
        {
            ["oportunidade"] = Oportunidade(),
            ["pontuacao"] = 85,
            ["motivos"] = new JsonArray(
                "Área de interesse: Tecnologia",
                "Oferecida no seu bairro: Capão Redondo",
                "Gratuita")
        },
        [typeof(DadosDetalhamentoInscricao)] = () => new JsonObject
        {
            ["id"] = 10,
            ["cidadaoId"] = 5,
            ["cidadaoNome"] = "Ana Souza",
            ["oportunidadeId"] = 3,
            ["oportunidadeTitulo"] = "Curso de Programação Web",
            ["status"] = "Pendente",
            ["criadaEm"] = "2026-11-11T12:30:00Z",
            ["atualizadaEm"] = null
        },
        [typeof(DadosCriacaoProposta)] = () => new JsonObject
        {
            ["titulo"] = "Oficina de eletricista residencial",
            ["descricao"] = "Muitos moradores querem aprender instalação elétrica básica e não há curso perto daqui.",
            ["area"] = "ConstrucaoCivil",
            ["bairro"] = "Capão Redondo"
        },
        [typeof(DadosDetalhamentoProposta)] = () => new JsonObject
        {
            ["id"] = 2,
            ["titulo"] = "Oficina de eletricista residencial",
            ["descricao"] = "Muitos moradores querem aprender instalação elétrica básica e não há curso perto daqui.",
            ["area"] = "ConstrucaoCivil",
            ["bairro"] = "Capão Redondo",
            ["status"] = "Aberta",
            ["totalVotos"] = 7,
            ["autorId"] = 5,
            ["autorNome"] = "Ana Souza",
            ["oportunidadeGeradaId"] = null,
            ["criadaEm"] = "2026-11-10T09:00:00Z"
        },
        [typeof(DadosRelatorioImpacto)] = () => new JsonObject
        {
            ["totalInscricoes"] = 9,
            ["inscricoesConcluidas"] = 3,
            ["taxaConclusaoPercentual"] = 33.3,
            ["propostasAtendidas"] = 2,
            ["porBairro"] = new JsonArray(new JsonObject
            {
                ["bairro"] = "Capão Redondo",
                ["oportunidadesAbertas"] = 5,
                ["vagasOfertadas"] = 89,
                ["inscricoesAtivas"] = 5,
                ["inscricoesConcluidas"] = 3,
                ["propostasAbertas"] = 1,
                ["propostasAtendidas"] = 1
            }),
            ["demandaReprimida"] = new JsonArray(new JsonObject
            {
                ["area"] = "Saude",
                ["oportunidadesAbertas"] = 0,
                ["propostasAbertas"] = 1,
                ["votosEmPropostasAbertas"] = 3
            })
        },
        [typeof(DadosErro)] = () => new JsonObject { ["mensagem"] = "Mensagem em português explicando o motivo do erro." },
        [typeof(DadosErroValidacao)] = () => new JsonObject { ["campo"] = "email", ["mensagem"] = "E-mail inválido" }
    };

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema concreto && Exemplos.TryGetValue(context.Type, out var criar))
            concreto.Example = criar();
    }
}

/// <summary>Descreve os parâmetros comuns (id, paginação) que não têm comentário XML próprio.</summary>
public sealed class DescricaoParametrosOperationFilter : IOperationFilter
{
    private static readonly Dictionary<string, string> Descricoes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = "Identificador do recurso.",
        ["pagina"] = "Número da página, começando em 1.",
        ["tamanho"] = "Itens por página, de 1 a 50 (padrão 10).",
        ["bairro"] = "Nome do bairro (busca exata).",
        ["area"] = "Área de conhecimento.",
        ["status"] = "Filtra pelo status.",
        ["cep"] = "CEP com ou sem hífen (ex.: 01001-000).",
        ["modalidade"] = "Presencial, Online ou Hibrida.",
        ["tipo"] = "Tipo (oportunidade: Curso, Oficina, Mentoria, JovemAprendiz; organização: Ong, EscolaTecnica, Empresa, PoderPublico, ColetivoComunitario).",
        ["gratuita"] = "true para só gratuitas, false para só pagas; omita para todas.",
        ["organizacaoId"] = "Mostra apenas as oportunidades desta organização.",
        ["texto"] = "Palavra buscada no título e na descrição.",
        ["somenteComVagas"] = "true para esconder oportunidades sem vagas disponíveis.",
        ["ordenarPorVotos"] = "true (padrão) ordena pelas mais votadas; false, pelas mais recentes.",
        ["limite"] = "Quantidade máxima de recomendações devolvidas (padrão 5)."
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null) return;
        foreach (var p in operation.Parameters)
            if (p is OpenApiParameter parametro && string.IsNullOrWhiteSpace(parametro.Description)
                && parametro.Name is not null && Descricoes.TryGetValue(parametro.Name, out var texto))
                parametro.Description = texto;
    }
}
