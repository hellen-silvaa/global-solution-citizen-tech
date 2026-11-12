using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oportuniza.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaControleDeLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "bloqueado_ate",
                table: "usuarios",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tentativas_login_falhas",
                table: "usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ultimo_login_em",
                table: "usuarios",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bloqueado_ate",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "tentativas_login_falhas",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "ultimo_login_em",
                table: "usuarios");
        }
    }
}
