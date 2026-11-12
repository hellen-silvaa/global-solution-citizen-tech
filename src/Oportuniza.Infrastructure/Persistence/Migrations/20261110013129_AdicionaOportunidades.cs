using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Oportuniza.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaOportunidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "oportunidades",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    organizacao_id = table.Column<long>(type: "bigint", nullable: false),
                    titulo = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    descricao = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    tipo = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    modalidade = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    area = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    bairro = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    carga_horaria = table.Column<int>(type: "int", nullable: false),
                    vagas_totais = table.Column<int>(type: "int", nullable: false),
                    vagas_ocupadas = table.Column<int>(type: "int", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    data_fim = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    gratuita = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oportunidades", x => x.id);
                    table.ForeignKey(
                        name: "FK_oportunidades_usuarios_organizacao_id",
                        column: x => x.organizacao_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_oportunidades_bairro",
                table: "oportunidades",
                column: "bairro");

            migrationBuilder.CreateIndex(
                name: "IX_oportunidades_organizacao_id",
                table: "oportunidades",
                column: "organizacao_id");

            migrationBuilder.CreateIndex(
                name: "IX_oportunidades_status_area",
                table: "oportunidades",
                columns: new[] { "status", "area" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "oportunidades");
        }
    }
}
