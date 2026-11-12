using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Oportuniza.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    nome = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    email = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    senha_hash = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    endereco_cep = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: true),
                    endereco_logradouro = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    endereco_numero = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    endereco_bairro = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    endereco_cidade = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    endereco_uf = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    tipo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    data_nascimento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    escolaridade = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    areas_interesse = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    tipo_organizacao = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    site = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    criado_em = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
