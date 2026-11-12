using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oportuniza.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Dados FICTÍCIOS para demonstração (nenhum dado pessoal real, como pede o briefing da GS).
    /// Todos os usuários usam a senha "Senha@123". As datas são relativas ao dia em que a
    /// migração é aplicada, para que as oportunidades estejam sempre no futuro na demonstração.
    /// </summary>
    public partial class SeedDadosFicticios : Migration
    {
        private const string SenhaPadrao = "Senha@123";

        private static readonly string[] ColunasUsuario =
        [
            "id", "tipo", "nome", "email", "senha_hash", "ativo", "criado_em",
            "endereco_cep", "endereco_logradouro", "endereco_numero", "endereco_bairro", "endereco_cidade", "endereco_uf",
            "data_nascimento", "escolaridade", "areas_interesse", "tipo_organizacao", "descricao", "site"
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var hoje = DateTime.UtcNow.Date;
            var hash = BCrypt.Net.BCrypt.HashPassword(SenhaPadrao);

            // ---------- Usuários (TPH) ----------
            migrationBuilder.InsertData("usuarios", ColunasUsuario, new object[,]
            {
                { 1L, "ADMIN", "Administração Oportuniza", "admin@oportuniza.local", hash, true, hoje,
                  null, null, null, null, null, null, null, null, null, null, null, null },

                { 2L, "ORGANIZACAO", "Instituto Recomeçar", "contato@recomecar.local", hash, true, hoje,
                  "05863000", "Estrada de Itapecerica", "1500", "Capão Redondo", "São Paulo", "SP",
                  null, null, null, "Ong", "ONG que oferece cursos gratuitos de tecnologia e empregabilidade para jovens da Zona Sul.", null },
                { 3L, "ORGANIZACAO", "Escola Técnica Zona Sul", "secretaria@etzonasul.local", hash, true, hoje,
                  "05765000", "Avenida Carlos Lacerda", "300", "Campo Limpo", "São Paulo", "SP",
                  null, null, null, "EscolaTecnica", "Escola técnica com cursos livres e bolsas integrais para moradores da região.", null },
                { 4L, "ORGANIZACAO", "Núcleo Municipal de Qualificação Sul", "nucleo.sul@qualifica.local", hash, true, hoje,
                  "04929000", "Estrada do M'Boi Mirim", "2000", "Jardim Ângela", "São Paulo", "SP",
                  null, null, null, "PoderPublico", "Equipamento público de qualificação profissional e encaminhamento ao primeiro emprego.", null },

                { 5L, "CIDADAO", "Ana Souza (fictícia)", "ana@exemplo.local", hash, true, hoje,
                  "05863000", "Rua das Flores", "45", "Capão Redondo", "São Paulo", "SP",
                  new DateTime(2006, 3, 14), "MedioCompleto", "Tecnologia,Administracao", null, null, null },
                { 6L, "CIDADAO", "Bruno Lima (fictício)", "bruno@exemplo.local", hash, true, hoje,
                  "05765000", "Rua do Campo", "120", "Campo Limpo", "São Paulo", "SP",
                  new DateTime(1998, 9, 2), "FundamentalCompleto", "Gastronomia,Empreendedorismo", null, null, null },
                { 7L, "CIDADAO", "Carla Mendes (fictícia)", "carla@exemplo.local", hash, true, hoje,
                  "04929000", "Rua da Paz", "8", "Jardim Ângela", "São Paulo", "SP",
                  new DateTime(1985, 6, 21), "MedioIncompleto", "Saude,Idiomas", null, null, null }
            });

            // ---------- Oportunidades ----------
            migrationBuilder.InsertData("oportunidades",
                ["id", "organizacao_id", "titulo", "descricao", "tipo", "modalidade", "area", "bairro",
                 "carga_horaria", "vagas_totais", "vagas_ocupadas", "data_inicio", "data_fim", "gratuita", "status", "criado_em"],
                new object[,]
                {
                    { 1L, 2L, "Introdução à Programação Web", "HTML, CSS e JavaScript do zero, com projeto final para o portfólio.",
                      "Curso", "Online", "Tecnologia", "Capão Redondo", 40, 30, 1, hoje.AddDays(20), hoje.AddDays(60), true, "Aberta", hoje },
                    { 2L, 2L, "Oficina de Currículo e Entrevista", "Monte seu currículo e treine entrevistas com recrutadores voluntários.",
                      "Oficina", "Presencial", "Administracao", "Capão Redondo", 4, 25, 1, hoje.AddDays(10), hoje.AddDays(10), true, "Aberta", hoje },
                    { 3L, 3L, "Auxiliar Administrativo", "Rotinas de escritório, planilhas e atendimento ao cliente.",
                      "Curso", "Presencial", "Administracao", "Campo Limpo", 80, 20, 0, hoje.AddDays(30), hoje.AddDays(90), true, "Aberta", hoje },
                    { 4L, 3L, "Cozinha Profissional Básica", "Boas práticas, técnicas de preparo e custos para quem quer trabalhar com comida.",
                      "Curso", "Presencial", "Gastronomia", "Campo Limpo", 60, 15, 1, hoje.AddDays(25), hoje.AddDays(70), true, "Aberta", hoje },
                    { 5L, 4L, "Jovem Aprendiz em Logística", "Programa de aprendizagem com contrato e formação teórica semanal.",
                      "JovemAprendiz", "Hibrida", "Logistica", "Jardim Ângela", 400, 10, 0, hoje.AddDays(40), hoje.AddDays(400), true, "Aberta", hoje },
                    { 6L, 4L, "Mentoria: Abra seu MEI", "Encontros online para formalizar e organizar o seu pequeno negócio.",
                      "Mentoria", "Online", "Empreendedorismo", "Jardim Ângela", 6, 50, 0, hoje.AddDays(15), hoje.AddDays(45), true, "Aberta", hoje },
                    { 7L, 2L, "Informática Básica", "Turma já concluída: computador, internet e serviços públicos digitais.",
                      "Curso", "Presencial", "Tecnologia", "Capão Redondo", 20, 20, 2, hoje.AddDays(-60), hoje.AddDays(-30), true, "Encerrada", hoje.AddDays(-90) }
                });

            // ---------- Inscrições ----------
            migrationBuilder.InsertData("inscricoes",
                ["id", "cidadao_id", "oportunidade_id", "status", "criado_em", "atualizada_em"],
                new object[,]
                {
                    { 1L, 5L, 1L, "Confirmada", hoje, hoje },
                    { 2L, 7L, 2L, "Pendente", hoje, null },
                    { 3L, 6L, 4L, "Pendente", hoje, null },
                    { 4L, 5L, 7L, "Concluida", hoje.AddDays(-70), hoje.AddDays(-30) },
                    { 5L, 7L, 7L, "Concluida", hoje.AddDays(-70), hoje.AddDays(-30) }
                });

            // ---------- Propostas e votos (participação cidadã) ----------
            migrationBuilder.InsertData("propostas",
                ["id", "autor_id", "titulo", "descricao", "area", "bairro", "status", "oportunidade_gerada_id", "criado_em"],
                new object[,]
                {
                    { 1L, 7L, "Curso de Cuidador de Idosos", "Muitas famílias do bairro precisam e não há formação perto.",
                      "Saude", "Jardim Ângela", "Aberta", null, hoje.AddDays(-5) },
                    { 2L, 6L, "Inglês para Atendimento", "Inglês básico para trabalhar no comércio e em restaurantes.",
                      "Idiomas", "Campo Limpo", "Aberta", null, hoje.AddDays(-3) },
                    { 3L, 5L, "Programação para Iniciantes", "Curso de programação gratuito e online para jovens do Capão.",
                      "Tecnologia", "Capão Redondo", "Atendida", 1L, hoje.AddDays(-40) }
                });

            migrationBuilder.InsertData("votos",
                ["id", "proposta_id", "cidadao_id", "criado_em"],
                new object[,]
                {
                    { 1L, 1L, 5L, hoje.AddDays(-4) },
                    { 2L, 1L, 6L, hoje.AddDays(-4) },
                    { 3L, 1L, 7L, hoje.AddDays(-5) },
                    { 4L, 2L, 7L, hoje.AddDays(-2) },
                    { 5L, 3L, 6L, hoje.AddDays(-39) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM votos WHERE id BETWEEN 1 AND 5;");
            migrationBuilder.Sql("DELETE FROM propostas WHERE id BETWEEN 1 AND 3;");
            migrationBuilder.Sql("DELETE FROM inscricoes WHERE id BETWEEN 1 AND 5;");
            migrationBuilder.Sql("DELETE FROM oportunidades WHERE id BETWEEN 1 AND 7;");
            migrationBuilder.Sql("DELETE FROM usuarios WHERE id BETWEEN 1 AND 7;");
        }
    }
}
