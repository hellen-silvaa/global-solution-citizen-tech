using Oportuniza.Domain.Common;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Usuarios;

namespace Oportuniza.Domain.Propostas;

/// <summary>
/// Participação cidadã (raiz de agregado): um morador propõe uma qualificação que falta no bairro,
/// a comunidade vota e uma organização atende a proposta publicando uma oportunidade.
/// Os votos fazem parte do agregado e só entram por <see cref="RegistrarVoto"/>.
/// </summary>
public class Proposta : Entidade, IRaizDeAgregado
{
    private readonly List<Voto> _votos = [];

    public long AutorId { get; private set; }
    public Cidadao Autor { get; private set; } = null!;
    public string Titulo { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public AreaConhecimento Area { get; private set; }
    public string Bairro { get; private set; } = null!;
    public StatusProposta Status { get; private set; } = StatusProposta.Aberta;
    public long? OportunidadeGeradaId { get; private set; }
    public Oportunidade? OportunidadeGerada { get; private set; }
    public IReadOnlyCollection<Voto> Votos => _votos.AsReadOnly();

    public int TotalVotos => _votos.Count;
    public bool EstaAberta => Status == StatusProposta.Aberta;

    private Proposta() { } // EF Core

    public Proposta(Cidadao autor, string titulo, string descricao, AreaConhecimento area, string bairro)
    {
        Autor = Guard.NaoNulo(autor, "Autor");
        if (!autor.Ativo)
            throw new ValidacaoException("Cidadão inativo não pode criar propostas!");
        AutorId = autor.Id;
        Titulo = Guard.Texto(titulo, "Título", 120);
        Descricao = Guard.Texto(descricao, "Descrição", 1000);
        Area = area;
        Bairro = Guard.Texto(bairro, "Bairro", 80);
    }

    /// <summary>RN-P1 (um voto por cidadão) e RN-P2 (só propostas abertas recebem voto).</summary>
    public Voto RegistrarVoto(Cidadao cidadao)
    {
        Guard.NaoNulo(cidadao, "Cidadão");
        if (!EstaAberta)
            throw new ValidacaoException("Só é possível votar em propostas abertas!");
        if (!cidadao.Ativo)
            throw new ValidacaoException("Cidadão inativo não pode votar!");
        if (_votos.Exists(v => v.CidadaoId == cidadao.Id))
            throw new ValidacaoException("Cidadão já votou nesta proposta!");

        var voto = new Voto(this, cidadao);
        _votos.Add(voto);
        return voto;
    }

    /// <summary>RN-P3: a organização atende a proposta publicando uma oportunidade.</summary>
    public void Atender(Oportunidade oportunidade)
    {
        Guard.NaoNulo(oportunidade, "Oportunidade");
        if (!EstaAberta)
            throw new ValidacaoException("Proposta não está aberta!");
        OportunidadeGerada = oportunidade;
        Status = StatusProposta.Atendida;
    }

    public void Arquivar()
    {
        if (!EstaAberta)
            throw new ValidacaoException("Só propostas abertas podem ser arquivadas!");
        Status = StatusProposta.Arquivada;
    }
}
