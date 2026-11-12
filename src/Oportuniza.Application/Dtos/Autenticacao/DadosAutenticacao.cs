using System.ComponentModel.DataAnnotations;
using Oportuniza.Domain.Enums;

namespace Oportuniza.Application.Dtos.Autenticacao;

/// <summary>Credenciais de login. Os limites de tamanho evitam abuso (senhas gigantes deixam o hash lento).</summary>
/// <param name="Email">E-mail cadastrado (até 120 caracteres).</param>
/// <param name="Senha">Senha da conta (até 64 caracteres).</param>
public record DadosLogin(
    [Required(ErrorMessage = "E-mail é obrigatório")] [StringLength(120)] string Email,
    [Required(ErrorMessage = "Senha é obrigatória")] [StringLength(64)] string Senha);

/// <summary>Token JWT devolvido pelo login.</summary>
/// <param name="Token">JWT assinado. Envie no cabeçalho Authorization: Bearer {token}.</param>
/// <param name="Tipo">Tipo do token (sempre Bearer).</param>
/// <param name="ExpiraEm">Data e hora de expiração do token (UTC).</param>
/// <param name="UsuarioId">Identificador do usuário autenticado.</param>
/// <param name="Nome">Nome do usuário.</param>
/// <param name="Perfil">Perfil de acesso: Admin, Cidadao ou Organizacao.</param>
public record DadosToken(string Token, string Tipo, DateTime ExpiraEm, long UsuarioId, string Nome, Perfil Perfil);
