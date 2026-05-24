using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class AddModeloMoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelosMotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeModelo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelosMotos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModelosMotos_Marca",
                table: "ModelosMotos",
                column: "Marca");

            migrationBuilder.CreateIndex(
                name: "IX_ModelosMotos_NomeModelo",
                table: "ModelosMotos",
                column: "NomeModelo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelosMotos");
        }
    }
}
