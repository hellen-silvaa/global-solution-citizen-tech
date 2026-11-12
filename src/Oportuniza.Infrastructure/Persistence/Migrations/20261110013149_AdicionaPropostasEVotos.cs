using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Oportuniza.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaPropostasEVotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "propostas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    autor_id = table.Column<long>(type: "bigint", nullable: false),
                    titulo = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    descricao = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    area = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    bairro = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    oportunidade_gerada_id = table.Column<long>(type: "bigint", nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propostas", x => x.id);
                    table.ForeignKey(
                        name: "FK_propostas_oportunidades_oportunidade_gerada_id",
                        column: x => x.oportunidade_gerada_id,
                        principalTable: "oportunidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_propostas_usuarios_autor_id",
                        column: x => x.autor_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "votos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    proposta_id = table.Column<long>(type: "bigint", nullable: false),
                    cidadao_id = table.Column<long>(type: "bigint", nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_votos", x => x.id);
                    table.ForeignKey(
                        name: "FK_votos_propostas_proposta_id",
                        column: x => x.proposta_id,
                        principalTable: "propostas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_votos_usuarios_cidadao_id",
                        column: x => x.cidadao_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_propostas_autor_id",
                table: "propostas",
                column: "autor_id");

            migrationBuilder.CreateIndex(
                name: "IX_propostas_oportunidade_gerada_id",
                table: "propostas",
                column: "oportunidade_gerada_id");

            migrationBuilder.CreateIndex(
                name: "IX_propostas_status_bairro",
                table: "propostas",
                columns: new[] { "status", "bairro" });

            migrationBuilder.CreateIndex(
                name: "IX_votos_cidadao_id",
                table: "votos",
                column: "cidadao_id");

            migrationBuilder.CreateIndex(
                name: "IX_votos_proposta_id_cidadao_id",
                table: "votos",
                columns: new[] { "proposta_id", "cidadao_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "votos");

            migrationBuilder.DropTable(
                name: "propostas");
        }
    }
}
