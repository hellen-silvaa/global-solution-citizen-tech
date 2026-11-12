using Oportuniza.Domain.Common;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Domain.Usuarios;

/// <summary>Pessoa que procura qualificação. Os interesses e o bairro alimentam as recomendações.</summary>
public class Cidadao : Usuario
{
    private List<AreaConhecimento> _areasInteresse = [];

    public DateTime DataNascimento { get; private set; }
    public Escolaridade Escolaridade { get; private set; }
    public IReadOnlyCollection<AreaConhecimento> AreasInteresse => _areasInteresse.AsReadOnly();

    public override Perfil Perfil => Perfil.Cidadao;

    private Cidadao() { } // EF Core

    public Cidadao(string nome, Email email, string senhaHash, Endereco endereco,
        DateTime dataNascimento, Escolaridade escolaridade, IEnumerable<AreaConhecimento> areasInteresse)
        : base(nome, email, senhaHash, Guard.NaoNulo(endereco, "Endereço"))
    {
        DataNascimento = ValidarNascimento(dataNascimento);
        Escolaridade = escolaridade;
        DefinirInteresses(areasInteresse);
    }

    public void AtualizarPerfil(Escolaridade? escolaridade, IEnumerable<AreaConhecimento>? areasInteresse)
    {
        if (escolaridade.HasValue)
            Escolaridade = escolaridade.Value;
        if (areasInteresse is not null)
            DefinirInteresses(areasInteresse);
    }

    public bool TemInteresseEm(AreaConhecimento area) => _areasInteresse.Contains(area);

    public int IdadeEm(DateTime data)
    {
        var idade = data.Year - DataNascimento.Year;
        if (DataNascimento.Date > data.Date.AddYears(-idade)) idade--;
        return idade;
    }

    private void DefinirInteresses(IEnumerable<AreaConhecimento>? areas)
    {
        var distintas = (areas ?? []).Distinct().ToList();
        if (distintas.Count == 0)
            throw new ValidacaoException("Informe ao menos uma área de interesse!");
        _areasInteresse = distintas;
    }

    private static DateTime ValidarNascimento(DateTime data)
    {
        if (data.Date >= DateTime.UtcNow.Date)
            throw new ValidacaoException("Data de nascimento deve estar no passado!");
        if (data.Year < 1900)
            throw new ValidacaoException("Data de nascimento inválida!");
        return data.Date;
    }
}
