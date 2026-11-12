using Microsoft.AspNetCore.Mvc;
using Oportuniza.Api.Infrastructure;
using Oportuniza.Application.Common;

namespace Oportuniza.Api.Controllers;

/// <summary>Base dos controllers: JSON, erros padronizados e acesso ao usuário do token.</summary>
[ApiController]
[Produces("application/json")]
[ProducesResponseType<DadosErro>(StatusCodes.Status500InternalServerError)]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Usuário autenticado (só usar em ações com [Authorize]).</summary>
    protected UsuarioAutenticado UsuarioLogado => User.ParaUsuarioAutenticado();

    /// <summary>Nomes dos perfis usados em [Authorize(Roles = ...)].</summary>
    protected static class Perfis
    {
        public const string Admin = nameof(Domain.Enums.Perfil.Admin);
        public const string Cidadao = nameof(Domain.Enums.Perfil.Cidadao);
        public const string Organizacao = nameof(Domain.Enums.Perfil.Organizacao);
    }
}
