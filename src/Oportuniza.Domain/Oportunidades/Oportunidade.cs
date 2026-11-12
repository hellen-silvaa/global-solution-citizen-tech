using Oportuniza.Domain.Common;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Inscricoes;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Domain.Oportunidades;

/// <summary>
/// Curso, oficina, mentoria ou vaga de jovem aprendiz publicada por uma organização (raiz de agregado).
/// Controla as próprias vagas e é a única porta de entrada para criar uma <see cref="Inscricao"/>,
/// garantindo as regras RN-I1, RN-I2, RN-I4 e RN-I5 em um só lugar.
/// </summary>
public class Oportunidade : Entidade, IRaizDeAgregado
{
    public long OrganizacaoId { get; private set; }
    public Organizacao Organizacao { get; private set; } = null!;
    public string Titulo { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public TipoOportunidade Tipo { get; private set; }
    public Modalidade Modalidade { get; private set; }
    public AreaConhecimento Area { get; private set; }
    public string Bairro { get; private set; } = null!;
    public int CargaHoraria { get; private set; }
    public int VagasTotais { get; private set; }
    public int VagasOcupadas { get; private set; }
    public Periodo Periodo { get; private set; } = null!;
    public bool Gratuita { get; private set; }
    public StatusOportunidade Status { get; private set; } = StatusOportunidade.Aberta;

    public int VagasDisponiveis => VagasTotais - VagasOcupadas;
    public bool TemVagas => VagasDisponiveis > 0;
    public bool EstaAberta => Status == StatusOportunidade.Aberta;

    private Oportunidade() { } // EF Core

    public Oportunidade(Organizacao organizacao, string titulo, string descricao, TipoOportunidade tipo,
        Modalidade modalidade, AreaConhecimento area, string bairro, int cargaHoraria, int vagas,
        Periodo periodo, bool gratuita)
    {
        Organizacao = Guard.NaoNulo(organizacao, "Organização");
        OrganizacaoId = organizacao.Id;
        Tipo = tipo;
        Gratuita = gratuita;
        DefinirDados(titulo, descricao, modalidade, area, bairro, cargaHoraria, vagas, periodo);
    }

    /// <summary>RN-O1: só a organização dona altera a oportunidade.</summary>
    public bool PertenceA(long organizacaoId) => OrganizacaoId == organizacaoId;

    /// <summary>RN-I1 + RN-I5: aberta e ainda não começou.</summary>
    public bool AceitaInscricoesEm(DateTime agora) => EstaAberta && !Periodo.JaComecouEm(agora);

    /// <summary>
    /// Inscreve um cidadão reservando uma vaga. Regras garantidas pelo agregado:
    /// RN-I1 (aberta), RN-I2 (vaga), RN-I4 (cidadão ativo) e RN-I5 (antes do início).
    /// </summary>
    public Inscricao Inscrever(Cidadao cidadao, DateTime agora)
    {
        Guard.NaoNulo(cidadao, "Cidadão");
        if (!cidadao.Ativo)
            throw new ValidacaoException("Cidadão inativo não pode se inscrever!");
        if (!EstaAberta)
            throw new ValidacaoException("Oportunidade não está aberta para inscrições!");
        if (Periodo.JaComecouEm(agora))
            throw new ValidacaoException("Prazo de inscrição encerrado: a oportunidade já começou!");

        ReservarVaga();
        return new Inscricao(cidadao, this);
    }

    public void Atualizar(string titulo, string descricao, Modalidade modalidade, AreaConhecimento area,
        string bairro, int cargaHoraria, int vagas, Periodo periodo)
    {
        if (!EstaAberta)
            throw new ValidacaoException("Só é possível editar oportunidades abertas!");
        if (vagas < VagasOcupadas)
            throw new ValidacaoException($"Já existem {VagasOcupadas} vagas ocupadas; o total não pode ser menor!");
        DefinirDados(titulo, descricao, modalidade, area, bairro, cargaHoraria, vagas, periodo);
    }

    public void Encerrar()
    {
        if (!EstaAberta)
            throw new ValidacaoException("Oportunidade já está encerrada ou cancelada!");
        Status = StatusOportunidade.Encerrada;
    }

    public void Cancelar()
    {
        if (Status == StatusOportunidade.Cancelada)
            throw new ValidacaoException("Oportunidade já está cancelada!");
        Status = StatusOportunidade.Cancelada;
    }

    /// <summary>Devolve a vaga quando uma inscrição é cancelada.</summary>
    internal void LiberarVaga()
    {
        if (VagasOcupadas > 0)
            VagasOcupadas--;
    }

    private void ReservarVaga()
    {
        if (!TemVagas)
            throw new ValidacaoException("Não há vagas disponíveis nesta oportunidade!");
        VagasOcupadas++;
    }

    private void DefinirDados(string titulo, string descricao, Modalidade modalidade, AreaConhecimento area,
        string bairro, int cargaHoraria, int vagas, Periodo periodo)
    {
        Titulo = Guard.Texto(titulo, "Título", 120);
        Descricao = Guard.Texto(descricao, "Descrição", 1000);
        Modalidade = modalidade;
        Area = area;
        Bairro = Guard.Texto(bairro, "Bairro", 80);
        CargaHoraria = Guard.Positivo(cargaHoraria, "Carga horária");
        VagasTotais = Guard.Positivo(vagas, "Número de vagas");
        Periodo = Guard.NaoNulo(periodo, "Período");
    }
}
