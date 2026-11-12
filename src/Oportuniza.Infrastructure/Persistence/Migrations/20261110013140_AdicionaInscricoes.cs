using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Oportuniza.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaInscricoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inscricoes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    cidadao_id = table.Column<long>(type: "bigint", nullable: false),
                    oportunidade_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    atualizada_em = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inscricoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_inscricoes_oportunidades_oportunidade_id",
                        column: x => x.oportunidade_id,
                        principalTable: "oportunidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inscricoes_usuarios_cidadao_id",
                        column: x => x.cidadao_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_inscricoes_cidadao_id_oportunidade_id",
                table: "inscricoes",
                columns: new[] { "cidadao_id", "oportunidade_id" });

            migrationBuilder.CreateIndex(
                name: "IX_inscricoes_oportunidade_id",
                table: "inscricoes",
                column: "oportunidade_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inscricoes");
        }
    }
}
