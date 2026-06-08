using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MotoRevApi.Data;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260608000000_AddRevisaoPadraoPecas")]
    public partial class AddRevisaoPadraoPecas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RevisoesPadrao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Quilometragem = table.Column<int>(type: "int", nullable: false),
                    TempoMeses = table.Column<int>(type: "int", nullable: false),
                    ModeloMotoId = table.Column<int>(type: "int", nullable: false),
                    ConcessionariaId = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisoesPadrao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevisoesPadrao_Concessionarias_ConcessionariaId",
                        column: x => x.ConcessionariaId,
                        principalTable: "Concessionarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevisoesPadrao_ModelosMotos_ModeloMotoId",
                        column: x => x.ModeloMotoId,
                        principalTable: "ModelosMotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevisaoPadraoPecas",
                columns: table => new
                {
                    RevisaoPadraoId = table.Column<int>(type: "int", nullable: false),
                    PecaId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisaoPadraoPecas", x => new { x.RevisaoPadraoId, x.PecaId });
                    table.ForeignKey(
                        name: "FK_RevisaoPadraoPecas_Pecas_PecaId",
                        column: x => x.PecaId,
                        principalTable: "Pecas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevisaoPadraoPecas_RevisoesPadrao_RevisaoPadraoId",
                        column: x => x.RevisaoPadraoId,
                        principalTable: "RevisoesPadrao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevisaoPadraoServicos",
                columns: table => new
                {
                    RevisaoPadraoId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisaoPadraoServicos", x => new { x.RevisaoPadraoId, x.ServicoId });
                    table.ForeignKey(
                        name: "FK_RevisaoPadraoServicos_RevisoesPadrao_RevisaoPadraoId",
                        column: x => x.RevisaoPadraoId,
                        principalTable: "RevisoesPadrao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevisaoPadraoServicos_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RevisaoPadraoPecas_PecaId",
                table: "RevisaoPadraoPecas",
                column: "PecaId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisaoPadraoServicos_ServicoId",
                table: "RevisaoPadraoServicos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisoesPadrao_ConcessionariaId",
                table: "RevisoesPadrao",
                column: "ConcessionariaId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisoesPadrao_ModeloMotoId",
                table: "RevisoesPadrao",
                column: "ModeloMotoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RevisaoPadraoPecas");
            migrationBuilder.DropTable(name: "RevisaoPadraoServicos");
            migrationBuilder.DropTable(name: "RevisoesPadrao");
        }
    }
}
