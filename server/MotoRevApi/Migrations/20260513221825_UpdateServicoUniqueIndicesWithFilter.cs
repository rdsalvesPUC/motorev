using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServicoUniqueIndicesWithFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Servicos_Categoria_Nome",
                table: "Servicos");

            migrationBuilder.DropIndex(
                name: "IX_Servicos_Codigo",
                table: "Servicos");

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_Categoria_Nome",
                table: "Servicos",
                columns: new[] { "Categoria", "Nome" },
                unique: true,
                filter: "[Ativo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_Codigo",
                table: "Servicos",
                column: "Codigo",
                unique: true,
                filter: "[Ativo] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Servicos_Categoria_Nome",
                table: "Servicos");

            migrationBuilder.DropIndex(
                name: "IX_Servicos_Codigo",
                table: "Servicos");

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_Categoria_Nome",
                table: "Servicos",
                columns: new[] { "Categoria", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_Codigo",
                table: "Servicos",
                column: "Codigo",
                unique: true);
        }
    }
}
