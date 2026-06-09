using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAgendamentoRevisaoMoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataAgendamento",
                table: "RevisoesMotos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LojaId",
                table: "RevisoesMotos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RevisoesMotos_LojaId",
                table: "RevisoesMotos",
                column: "LojaId");

            migrationBuilder.AddForeignKey(
                name: "FK_RevisoesMotos_Lojas_LojaId",
                table: "RevisoesMotos",
                column: "LojaId",
                principalTable: "Lojas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RevisoesMotos_Lojas_LojaId",
                table: "RevisoesMotos");

            migrationBuilder.DropIndex(
                name: "IX_RevisoesMotos_LojaId",
                table: "RevisoesMotos");

            migrationBuilder.DropColumn(
                name: "DataAgendamento",
                table: "RevisoesMotos");

            migrationBuilder.DropColumn(
                name: "LojaId",
                table: "RevisoesMotos");
        }
    }
}
