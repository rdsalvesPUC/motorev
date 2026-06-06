using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLinhaEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Linha",
                table: "ModelosMotos");

            migrationBuilder.AddColumn<int>(
                name: "LinhaId",
                table: "ModelosMotos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Linhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Linhas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModelosMotos_LinhaId",
                table: "ModelosMotos",
                column: "LinhaId");

            migrationBuilder.CreateIndex(
                name: "IX_Linhas_Nome",
                table: "Linhas",
                column: "Nome",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelosMotos_Linhas_LinhaId",
                table: "ModelosMotos",
                column: "LinhaId",
                principalTable: "Linhas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModelosMotos_Linhas_LinhaId",
                table: "ModelosMotos");

            migrationBuilder.DropTable(
                name: "Linhas");

            migrationBuilder.DropIndex(
                name: "IX_ModelosMotos_LinhaId",
                table: "ModelosMotos");

            migrationBuilder.DropColumn(
                name: "LinhaId",
                table: "ModelosMotos");

            migrationBuilder.AddColumn<string>(
                name: "Linha",
                table: "ModelosMotos",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
