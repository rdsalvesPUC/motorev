using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRevisoesMotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RevisoesMotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MotoId = table.Column<int>(type: "int", nullable: false),
                    RevisaoPadraoId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Quilometragem = table.Column<int>(type: "int", nullable: false),
                    TempoMeses = table.Column<int>(type: "int", nullable: false),
                    DataPrevista = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisoesMotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevisoesMotos_Motos_MotoId",
                        column: x => x.MotoId,
                        principalTable: "Motos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisoesMotos_RevisoesPadrao_RevisaoPadraoId",
                        column: x => x.RevisaoPadraoId,
                        principalTable: "RevisoesPadrao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RevisoesMotos_MotoId",
                table: "RevisoesMotos",
                column: "MotoId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisoesMotos_MotoId_Ordem",
                table: "RevisoesMotos",
                columns: new[] { "MotoId", "Ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RevisoesMotos_RevisaoPadraoId",
                table: "RevisoesMotos",
                column: "RevisaoPadraoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RevisoesMotos");
        }
    }
}
