using Microsoft.EntityFrameworkCore;
using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Inscricoes;
using Oportuniza.Application.Dtos.Oportunidades;
using Oportuniza.Application.Dtos.Propostas;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Propostas;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;
using Oportuniza.Infrastructure.Persistence;
using Oportuniza.Infrastructure.Persistence.Queries;
using Oportuniza.Infrastructure.Persistence.Repositories;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Infrastructure;

/// <summary>
/// Cenário fixo gravado no SQLite:
/// org 20 (ONG) e org 21 (empresa, inativa); cidadãos 10 (Capão, Tecnologia) e 11 (Centro, Artes) e 12 (inativo);
/// oportunidades 1 (Tecnologia/Online/Capão, 2 vagas), 2 (Artes/Presencial/Centro, paga), 3 (encerrada), 4 (já começou);
/// inscrições: 10→1 pendente, 11→1 concluída, 11→2 cancelada; propostas 50 (2 votos), 51 (0 votos, atendida), 52 (1 voto).
/// </summary>
public sealed class RepositoriosTests : IDisposable
{
    private readonly BancoEmMemoria _banco = new();

    public RepositoriosTests()
    {
        using var db = _banco.NovoContexto();

        var ong = Fabrica.Organizacao(20);
        var empresa = Fabrica.ComId(new Organizacao("Empresa X", Email.Criar("empresa@teste.local"), "hash",
            Fabrica.Endereco("Centro"), TipoOrganizacao.Empresa, "Empresa", null), 21);
        empresa.Desativar();

        var ana = Fabrica.Cidadao(10, "Capão Redondo", AreaConhecimento.Tecnologia, AreaConhecimento.Idiomas);
        var bia = Fabrica.Cidadao(11, "Centro", AreaConhecimento.Artes);
        var inativo = Fabrica.Cidadao(12, "Centro");
        inativo.Desativar();

        var tecnologia = Fabrica.Oportunidade(1, ong, vagas: 2, modalidade: Modalidade.Online);
        var artes = Fabrica.ComId(new Oportunidade(ong, "Oficina de Pintura", "Cores e telas", TipoOportunidade.Oficina,
            Modalidade.Presencial, AreaConhecimento.Artes, "Centro", 8, 10, Fabrica.Periodo(5), gratuita: false), 2);
        var encerrada = Fabrica.Oportunidade(3, ong);
        encerrada.Encerrar();
        var jaComecou = Fabrica.Oportunidade(4, ong, diasParaInicio: -2);

        var pendente = tecnologia.Inscrever(ana, Fabrica.Agora);
        var concluida = tecnologia.Inscrever(bia, Fabrica.Agora);
        concluida.Confirmar();
        concluida.Concluir();
        var cancelada = artes.Inscrever(bia, Fabrica.Agora);
        cancelada.Cancelar(Fabrica.Agora);

        var maisVotada = Fabrica.ComId(new Proposta(ana, "Cuidador de idosos", "Falta", AreaConhecimento.Saude, "Capão Redondo"), 50);
        maisVotada.RegistrarVoto(ana);
        maisVotada.RegistrarVoto(bia);
        var atendida = Fabrica.ComId(new Proposta(bia, "Pintura", "Queremos", AreaConhecimento.Artes, "Centro"), 51);
        atendida.Atender(artes);
        var umVoto = Fabrica.ComId(new Proposta(bia, "Inglês", "Para o comércio", AreaConhecimento.Idiomas, "Centro"), 52);
        umVoto.RegistrarVoto(ana);

        db.AddRange(ong, empresa, ana, bia, inativo, tecnologia, artes, encerrada, jaComecou);
        db.AddRange(pendente, concluida, cancelada);
        db.AddRange(maisVotada, atendida, umVoto);
        db.SaveChanges();
    }

    public void Dispose() => _banco.Dispose();

    // ---------- Usuários ----------

    [Fact]
    public async Task Usuario_BuscaPorEmailVOEDevolveOTipoConcreto()
    {
        using var db = _banco.NovoContexto();
        var repositorio = new UsuarioRepository(db);

        Assert.IsType<Cidadao>(await repositorio.ObterPorEmailAsync(Email.Criar("CIDADAO10@teste.local")));
        Assert.IsType<Organizacao>(await repositorio.ObterPorIdAsync(20));
        Assert.Null(await repositorio.ObterPorEmailAsync(Email.Criar("ninguem@teste.local")));
        Assert.True(await repositorio.EmailEmUsoAsync(Email.Criar("org20@teste.local")));
        Assert.False(await repositorio.EmailEmUsoAsync(Email.Criar("livre@teste.local")));
    }

    [Fact]
    public async Task Cidadao_PersisteInteressesEEndereco()
    {
        using var db = _banco.NovoContexto();

        var ana = await new CidadaoRepository(db).ObterPorIdAsync(10);

        Assert.Equal([AreaConhecimento.Tecnologia, AreaConhecimento.Idiomas], ana!.AreasInteresse);
        Assert.Equal("Capão Redondo", ana.Endereco!.Bairro);
        Assert.Equal("cidadao10@teste.local", ana.Email.Valor);
    }

    [Fact]
    public async Task Cidadao_ListaSoAtivosComFiltroDeBairro()
    {
        using var db = _banco.NovoContexto();
        var repositorio = new CidadaoRepository(db);

        Assert.Equal(2, (await repositorio.ListarAsync(null, new Paginacao())).TotalItens);
        Assert.Equal([11L], (await repositorio.ListarAsync(" Centro ", new Paginacao())).Itens.Select(c => c.Id));
    }

    [Fact]
    public async Task Organizacao_ListaSoAtivasComFiltroDeTipo()
    {
        using var db = _banco.NovoContexto();
        var repositorio = new OrganizacaoRepository(db);

        Assert.Equal([20L], (await repositorio.ListarAsync(null, new Paginacao())).Itens.Select(o => o.Id));
        Assert.Empty((await repositorio.ListarAsync(TipoOrganizacao.Empresa, new Paginacao())).Itens);
    }

    [Fact]
    public async Task Usuario_EmailDuplicado_BloqueadoPeloIndiceUnico()
    {
        using var db = _banco.NovoContexto();
        db.Add(new Administrador("Outro", Email.Criar("cidadao10@teste.local"), "hash"));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    // ---------- Oportunidades ----------

    public static TheoryData<FiltroOportunidades, long[]> Filtros => new()
    {
        { new FiltroOportunidades(), [4, 2, 1, 3] }, // abertas primeiro, pela data de início; depois a encerrada
        { new FiltroOportunidades(Area: AreaConhecimento.Artes), [2] },
        { new FiltroOportunidades(Modalidade: Modalidade.Online), [1] },
        { new FiltroOportunidades(Tipo: TipoOportunidade.Oficina), [2] },
        { new FiltroOportunidades(Gratuita: false), [2] },
        { new FiltroOportunidades(OrganizacaoId: 21), [] },
        { new FiltroOportunidades(Bairro: " Centro "), [2] },
        { new FiltroOportunidades(Texto: "Pintura"), [2] },
        { new FiltroOportunidades(Texto: "telas"), [2] },
        { new FiltroOportunidades(SomenteComVagas: true), [4, 2] }, // a 1 está lotada e a 3 encerrada
        { new FiltroOportunidades(Texto: "' OR 1=1 --"), [] },
        { new FiltroOportunidades(Texto: "x'; DROP TABLE oportunidades; --"), [] }
    };

    [Theory]
    [MemberData(nameof(Filtros))]
    public async Task Oportunidade_BuscaComCadaFiltro(FiltroOportunidades filtro, long[] esperados)
    {
        using var db = _banco.NovoContexto();

        var pagina = await new OportunidadeRepository(db).BuscarAsync(filtro, new Paginacao(1, 50));

        Assert.Equal(esperados, pagina.Itens.Select(o => o.Id));
        Assert.Equal(4, await db.Oportunidades.CountAsync()); // nenhuma injeção de SQL apagou dados
    }

    [Fact]
    public async Task Oportunidade_BuscaPagina()
    {
        using var db = _banco.NovoContexto();

        var pagina = await new OportunidadeRepository(db).BuscarAsync(new FiltroOportunidades(), new Paginacao(2, 3));

        Assert.Equal(4, pagina.TotalItens);
        Assert.Equal(2, pagina.TotalPaginas);
        Assert.Single(pagina.Itens);
    }

    [Fact]
    public async Task Oportunidade_ObterCarregaOrganizacaoEPeriodo()
    {
        using var db = _banco.NovoContexto();

        var oportunidade = await new OportunidadeRepository(db).ObterPorIdAsync(1);

        Assert.Equal("ONG Teste", oportunidade!.Organizacao.Nome);
        Assert.Equal(Fabrica.Agora.AddDays(10), oportunidade.Periodo.Inicio);
        Assert.Equal(2, oportunidade.VagasOcupadas); // pendente + concluída ocupam vaga; só o cancelamento devolve
    }

    [Fact]
    public async Task Oportunidade_ListarDisponiveis_SoAbertasComVagaENoPrazo()
    {
        using var db = _banco.NovoContexto();

        var disponiveis = await new OportunidadeRepository(db).ListarDisponiveisAsync(Fabrica.Agora);

        Assert.Equal([2L], disponiveis.Select(o => o.Id).Order()); // 1 está lotada, 3 encerrada, 4 já começou
    }

    [Fact]
    public async Task Oportunidade_UltimaVagaDisputada_TokenDeConcorrenciaBarraOSegundo()
    {
        using (var db = _banco.NovoContexto())
        {
            var artes = await db.Oportunidades.FirstAsync(o => o.Id == 2);
            artes.Atualizar(artes.Titulo, artes.Descricao, artes.Modalidade, artes.Area, artes.Bairro, artes.CargaHoraria, 1, artes.Periodo);
            await db.SaveChangesAsync();
        }

        using var primeiro = _banco.NovoContexto();
        using var segundo = _banco.NovoContexto();
        var op1 = await new OportunidadeRepository(primeiro).ObterPorIdAsync(2);
        var op2 = await new OportunidadeRepository(segundo).ObterPorIdAsync(2);
        primeiro.Add(op1!.Inscrever((await primeiro.Cidadaos.FindAsync(10L))!, Fabrica.Agora));
        segundo.Add(op2!.Inscrever((await segundo.Cidadaos.FindAsync(11L))!, Fabrica.Agora));

        await primeiro.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => segundo.SaveChangesAsync());
    }

    // ---------- Inscrições ----------

    [Fact]
    public async Task Inscricao_ConsultasDeInscricoesAtivas()
    {
        using var db = _banco.NovoContexto();
        var repositorio = new InscricaoRepository(db);

        Assert.True(await repositorio.ExisteInscricaoAtivaAsync(10, 1));
        Assert.False(await repositorio.ExisteInscricaoAtivaAsync(11, 1)); // concluída não conta
        Assert.False(await repositorio.ExisteInscricaoAtivaAsync(11, 2)); // cancelada não conta
        Assert.Equal(1, await repositorio.ContarAtivasDoCidadaoAsync(10));
        Assert.Equal(0, await repositorio.ContarAtivasDoCidadaoAsync(11));
        Assert.Equal([1L], await repositorio.ListarOportunidadesComInscricaoAtivaAsync(10));
    }

    [Fact]
    public async Task Inscricao_ListagensEDetalhe()
    {
        using var db = _banco.NovoContexto();
        var repositorio = new InscricaoRepository(db);

        Assert.Equal(2, (await repositorio.ListarPorCidadaoAsync(11, null)).Count);
        Assert.Equal(StatusInscricao.Concluida, Assert.Single(await repositorio.ListarPorCidadaoAsync(11, StatusInscricao.Concluida)).Status);
        Assert.Equal(2, (await repositorio.ListarPorOportunidadeAsync(1)).Count);

        var inscricao = (await repositorio.ListarPorCidadaoAsync(10, null))[0];
        var detalhe = await repositorio.ObterPorIdAsync(inscricao.Id);
        Assert.Equal("Cidadã Teste", detalhe!.Cidadao.Nome);
        Assert.Equal("Curso de teste", detalhe.Oportunidade.Titulo);
    }

    // ---------- Propostas ----------

    [Fact]
    public async Task Proposta_ObterCarregaVotosEAutor()
    {
        using var db = _banco.NovoContexto();

        var proposta = await new PropostaRepository(db).ObterPorIdAsync(50);

        Assert.Equal(2, proposta!.TotalVotos);
        Assert.Equal(10, proposta.Autor.Id);
    }

    [Fact]
    public async Task Proposta_ListaPorVotosOuPorData_EFiltra()
    {
        using var db = _banco.NovoContexto();
        var repositorio = new PropostaRepository(db);

        Assert.Equal([50L, 52L, 51L], (await repositorio.ListarAsync(new FiltroPropostas(), new Paginacao())).Itens.Select(p => p.Id));
        Assert.Equal([52L, 51L, 50L], (await repositorio.ListarAsync(new FiltroPropostas(OrdenarPorVotos: false), new Paginacao())).Itens.Select(p => p.Id));
        Assert.Equal([51L], (await repositorio.ListarAsync(new FiltroPropostas(Status: StatusProposta.Atendida), new Paginacao())).Itens.Select(p => p.Id));
        Assert.Equal([52L], (await repositorio.ListarAsync(new FiltroPropostas(Area: AreaConhecimento.Idiomas), new Paginacao())).Itens.Select(p => p.Id));
        Assert.Equal([50L], (await repositorio.ListarAsync(new FiltroPropostas(Bairro: "Capão Redondo"), new Paginacao())).Itens.Select(p => p.Id));
    }

    [Fact]
    public void Voto_Duplicado_BloqueadoTambemPeloIndiceUnico()
    {
        // O domínio já impede (RN-P1); o índice único é a segunda barreira, caso alguém grave direto no banco.
        using var db = _banco.NovoContexto();

        var erro = Assert.Throws<Microsoft.Data.Sqlite.SqliteException>(() =>
            db.Database.ExecuteSqlRaw("INSERT INTO votos (proposta_id, cidadao_id, criado_em) VALUES (52, 10, '2026-01-01')"));
        Assert.Contains("UNIQUE", erro.Message);
    }

    [Fact]
    public async Task Dtos_ToleramNavegacoesNaoCarregadas()
    {
        // Listagens leves não fazem Include de tudo: o DTO devolve nulo em vez de quebrar.
        using var db = _banco.NovoContexto();
        var inscricoes = new InscricaoRepository(db);

        var doCidadao = DadosDetalhamentoInscricao.De((await inscricoes.ListarPorCidadaoAsync(10, null))[0]);
        var daOportunidade = DadosDetalhamentoInscricao.De((await inscricoes.ListarPorOportunidadeAsync(1))[0]);
        var oportunidade = DadosDetalhamentoOportunidade.De(await db.Oportunidades.AsNoTracking().FirstAsync(o => o.Id == 2));
        var proposta = DadosDetalhamentoProposta.De(await db.Propostas.AsNoTracking().FirstAsync(p => p.Id == 51));

        Assert.Null(doCidadao.CidadaoNome);
        Assert.Equal("Curso de teste", doCidadao.OportunidadeTitulo);
        Assert.Null(daOportunidade.OportunidadeTitulo);
        Assert.NotNull(daOportunidade.CidadaoNome);
        Assert.Null(oportunidade.OrganizacaoNome);
        Assert.Null(proposta.AutorNome);
        Assert.Equal(2, proposta.OportunidadeGeradaId); // vem da chave estrangeira, sem carregar a oportunidade
    }

    // ---------- Consultas de indicadores ----------

    [Fact]
    public async Task Indicadores_AgrupamPorBairroEArea()
    {
        using var db = _banco.NovoContexto();
        var queries = new IndicadorQueries(db);

        var bairros = (await queries.ImpactoPorBairroAsync()).ToDictionary(b => b.Bairro);
        var areas = (await queries.DemandaPorAreaAsync()).ToDictionary(a => a.Area);

        Assert.Equal(1, bairros["Capão Redondo"].InscricoesAtivas);
        Assert.Equal(1, bairros["Capão Redondo"].InscricoesConcluidas);
        Assert.Equal(1, bairros["Capão Redondo"].PropostasAbertas);
        Assert.Equal(1, bairros["Centro"].PropostasAtendidas);
        Assert.Equal(1, bairros["Centro"].OportunidadesAbertas);
        Assert.Equal(Enum.GetValues<AreaConhecimento>().Length, areas.Count);
        Assert.Equal(2, areas[AreaConhecimento.Saude].VotosEmPropostasAbertas);
        Assert.Equal(0, areas[AreaConhecimento.Artes].PropostasAbertas);
    }

    [Fact]
    public async Task UnitOfWork_SalvaAlteracoes()
    {
        using (var db = _banco.NovoContexto())
        {
            var ana = await db.Cidadaos.FirstAsync(c => c.Id == 10);
            ana.AtualizarDados("Ana Atualizada", null);
            Assert.Equal(1, await ((IUnitOfWork)db).SalvarAsync());
        }

        using var outro = _banco.NovoContexto();
        Assert.Equal("Ana Atualizada", (await outro.Cidadaos.FirstAsync(c => c.Id == 10)).Nome);
    }
}
