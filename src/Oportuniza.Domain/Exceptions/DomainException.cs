namespace Oportuniza.Domain.Exceptions;

/// <summary>
/// Base das exceções da aplicação. Cada subclasse representa uma categoria de erro
/// que a Api traduz para um status HTTP (ver GlobalExceptionHandler).
/// </summary>
public abstract class DomainException(string mensagem, Exception? causa = null) : Exception(mensagem, causa);

/// <summary>Regra de negócio violada ou dado inválido (400).</summary>
public class ValidacaoException(string mensagem) : DomainException(mensagem);

/// <summary>Recurso inexistente (404).</summary>
public class RecursoNaoEncontradoException(string mensagem) : DomainException(mensagem);

/// <summary>Usuário autenticado sem permissão sobre o recurso (403).</summary>
public class AcessoNegadoException(string mensagem = "Acesso negado!") : DomainException(mensagem);

/// <summary>Credenciais inválidas (401). A mensagem é sempre a mesma para não revelar quais e-mails existem.</summary>
public class CredenciaisInvalidasException() : DomainException("Login ou senha inválidos!");

/// <summary>Conta temporariamente bloqueada por excesso de tentativas de login (429).</summary>
public class MuitasTentativasException()
    : DomainException("Muitas tentativas de login. Tente novamente em alguns minutos!");

/// <summary>Falha ao chamar um serviço externo, como o ViaCEP (503).</summary>
public class IntegracaoExternaException(string mensagem, Exception? causa = null) : DomainException(mensagem, causa);
