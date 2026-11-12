namespace Oportuniza.Domain.Common;

/// <summary>Base de todas as entidades: identidade e data de criação.</summary>
public abstract class Entidade
{
    public long Id { get; protected set; }
    public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;
}

/// <summary>
/// Marca a raiz de um agregado (DDD). Só raízes de agregado têm repositório e são carregadas/salvas
/// diretamente; as demais entidades (ex.: <c>Voto</c>) são alteradas apenas através da raiz.
/// </summary>
public interface IRaizDeAgregado;
