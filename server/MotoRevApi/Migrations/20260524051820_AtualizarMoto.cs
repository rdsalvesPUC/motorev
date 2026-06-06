using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarMoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cor",
                table: "Motos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataVenda",
                table: "Motos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "KilometragemAtual",
                table: "Motos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ano",
                table: "ModelosMotos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cilindrada",
                table: "ModelosMotos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Linha",
                table: "ModelosMotos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cor",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "DataVenda",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "KilometragemAtual",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "Ano",
                table: "ModelosMotos");

            migrationBuilder.DropColumn(
                name: "Cilindrada",
                table: "ModelosMotos");

            migrationBuilder.DropColumn(
                name: "Linha",
                table: "ModelosMotos");
        }
    }
}
