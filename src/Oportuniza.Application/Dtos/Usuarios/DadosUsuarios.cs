using System.ComponentModel.DataAnnotations;
using Oportuniza.Application.Common.Validation;
using Oportuniza.Application.Dtos.Enderecos;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Usuarios;

namespace Oportuniza.Application.Dtos.Usuarios;

// ---------- Cidadão ----------

/// <summary>Dados para o cidadão criar a conta (endpoint público).</summary>
/// <param name="Nome">Nome completo (até 120 caracteres).</param>
/// <param name="Email">E-mail único, usado no login.</param>
/// <param name="Senha">Senha de 8 a 64 caracteres, com letras e números, sem espaços nas pontas.</param>
/// <param name="DataNascimento">Data de nascimento (usada só para calcular a faixa etária).</param>
/// <param name="Escolaridade">Nível de escolaridade atual.</param>
/// <param name="AreasInteresse">De 1 a 10 áreas de conhecimento que interessam ao cidadão (base da recomendação).</param>
/// <param name="Endereco">Endereço do cidadão; o bairro entra no cálculo da recomendação.</param>
public record DadosCadastroCidadao(
    [Required(ErrorMessage = "Nome é obrigatório")] [StringLength(120)] string Nome,
    [Required(ErrorMessage = "E-mail é obrigatório")] [EmailAddress(ErrorMessage = "E-mail inválido")] [StringLength(120)] string Email,
    [Required(ErrorMessage = "Senha é obrigatória")] [SenhaForte] string Senha,
    [Required(ErrorMessage = "Data de nascimento é obrigatória")] DateTime DataNascimento,
    [Required(ErrorMessage = "Escolaridade é obrigatória")] Escolaridade Escolaridade,
    [Required(ErrorMessage = "Informe ao menos uma área de interesse")] [MinLength(1, ErrorMessage = "Informe ao menos uma área de interesse")] [MaxLength(10)] List<AreaConhecimento> AreasInteresse,
    [Required(ErrorMessage = "Endereço é obrigatório")] DadosEndereco Endereco);

/// <summary>DadosAtualizacaoCidadao</summary>
/// <param name="Nome">Novo nome (omita para manter).</param>
/// <param name="Escolaridade">Nova escolaridade (omita para manter).</param>
/// <param name="AreasInteresse">Nova lista de áreas de interesse, até 10 (omita para manter).</param>
/// <param name="Endereco">Novo endereço (omita para manter).</param>
public record DadosAtualizacaoCidadao(
    [StringLength(120)] string? Nome,
    Escolaridade? Escolaridade,
    [MaxLength(10)] List<AreaConhecimento>? AreasInteresse,
    DadosEndereco? Endereco);

/// <summary>Dados do cidadão logado.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Nome">Nome completo.</param>
/// <param name="Email">E-mail de login.</param>
/// <param name="DataNascimento">Data de nascimento.</param>
/// <param name="Escolaridade">Escolaridade.</param>
/// <param name="AreasInteresse">Áreas de interesse.</param>
/// <param name="Endereco">Endereço cadastrado.</param>
/// <param name="Ativo">Se a conta está ativa.</param>
/// <param name="CriadoEm">Data de criação da conta (UTC).</param>
public record DadosDetalhamentoCidadao(
    long Id, string Nome, string Email, DateTime DataNascimento, Escolaridade Escolaridade,
    IReadOnlyCollection<AreaConhecimento> AreasInteresse, DadosEndereco? Endereco, bool Ativo, DateTime CriadoEm)
{
    public static DadosDetalhamentoCidadao De(Cidadao c) => new(
        c.Id, c.Nome, c.Email.Valor, c.DataNascimento, c.Escolaridade, c.AreasInteresse,
        DadosEndereco.De(c.Endereco), c.Ativo, c.CriadoEm);
}

/// <summary>Listagem para o admin: sem e-mail nem data de nascimento (minimização de dados pessoais, LGPD).</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Nome">Nome.</param>
/// <param name="Bairro">Bairro de residência.</param>
/// <param name="Escolaridade">Escolaridade.</param>
/// <param name="Ativo">Se a conta está ativa.</param>
public record DadosListagemCidadao(long Id, string Nome, string Bairro, Escolaridade Escolaridade, bool Ativo)
{
    // Endereço é obrigatório para cidadão (invariante garantida no construtor de Cidadao).
    public static DadosListagemCidadao De(Cidadao c) => new(c.Id, c.Nome, c.Endereco!.Bairro, c.Escolaridade, c.Ativo);
}

// ---------- Organização ----------

/// <summary>Dados para uma organização criar a conta e passar a publicar oportunidades.</summary>
/// <param name="Nome">Nome da organização (até 120 caracteres).</param>
/// <param name="Email">E-mail único, usado no login.</param>
/// <param name="Senha">Senha de 8 a 64 caracteres, com letras e números.</param>
/// <param name="Tipo">Tipo: Ong, EscolaTecnica, Empresa, PoderPublico ou ColetivoComunitario.</param>
/// <param name="Descricao">Quem é a organização e o que oferece (até 500 caracteres).</param>
/// <param name="Site">Site da organização (opcional).</param>
/// <param name="Endereco">Endereço da organização.</param>
public record DadosCadastroOrganizacao(
    [Required(ErrorMessage = "Nome é obrigatório")] [StringLength(120)] string Nome,
    [Required(ErrorMessage = "E-mail é obrigatório")] [EmailAddress(ErrorMessage = "E-mail inválido")] [StringLength(120)] string Email,
    [Required(ErrorMessage = "Senha é obrigatória")] [SenhaForte] string Senha,
    [Required(ErrorMessage = "Tipo é obrigatório")] TipoOrganizacao Tipo,
    [Required(ErrorMessage = "Descrição é obrigatória")] [StringLength(500)] string Descricao,
    [StringLength(200)] string? Site,
    [Required(ErrorMessage = "Endereço é obrigatório")] DadosEndereco Endereco);

/// <summary>DadosAtualizacaoOrganizacao</summary>
/// <param name="Nome">Novo nome (omita para manter).</param>
/// <param name="Descricao">Nova descrição (omita para manter).</param>
/// <param name="Site">Novo site (omita para manter).</param>
/// <param name="Endereco">Novo endereço (omita para manter).</param>
public record DadosAtualizacaoOrganizacao(
    [StringLength(120)] string? Nome,
    [StringLength(500)] string? Descricao,
    [StringLength(200)] string? Site,
    DadosEndereco? Endereco);

/// <summary>Dados públicos de uma organização.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Nome">Nome.</param>
/// <param name="Email">E-mail de contato.</param>
/// <param name="Tipo">Tipo da organização.</param>
/// <param name="Descricao">Descrição.</param>
/// <param name="Site">Site (opcional).</param>
/// <param name="Endereco">Endereço.</param>
/// <param name="Ativo">Se a conta está ativa.</param>
/// <param name="CriadoEm">Data de criação (UTC).</param>
public record DadosDetalhamentoOrganizacao(
    long Id, string Nome, string Email, TipoOrganizacao Tipo, string Descricao, string? Site,
    DadosEndereco? Endereco, bool Ativo, DateTime CriadoEm)
{
    public static DadosDetalhamentoOrganizacao De(Organizacao o) => new(
        o.Id, o.Nome, o.Email.Valor, o.TipoOrganizacao, o.Descricao, o.Site, DadosEndereco.De(o.Endereco), o.Ativo, o.CriadoEm);
}
