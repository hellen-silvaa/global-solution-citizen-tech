using Oportuniza.Application.Dtos.Autenticacao;
using Oportuniza.Domain.Usuarios;

namespace Oportuniza.Application.Ports.Services;

/// <summary>Emissão de tokens de acesso.</summary>
public interface ITokenService
{
    DadosToken Gerar(Usuario usuario);
}
