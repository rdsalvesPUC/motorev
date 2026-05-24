using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Servicos_Nome_Categoria",
                table: "Servicos");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Servicos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_Categoria_Nome",
                table: "Servicos",
                columns: new[] { "Categoria", "Nome" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Servicos_Categoria_Nome",
                table: "Servicos");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Servicos");

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_Nome_Categoria",
                table: "Servicos",
                columns: new[] { "Nome", "Categoria" },
                unique: true);
        }
    }
}
