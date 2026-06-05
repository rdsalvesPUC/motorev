using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class AjustarPecaParaCadastro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Pecas");

            migrationBuilder.RenameColumn(
                name: "Valor",
                table: "Pecas",
                newName: "Preco");

            migrationBuilder.Sql("UPDATE Pecas SET Preco = 0 WHERE Preco IS NULL");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Pecas",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Preco",
                table: "Pecas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Pecas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Filtros");

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Pecas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql("UPDATE Pecas SET Codigo = CONCAT('P', RIGHT(CONCAT('000000', Id), 6)) WHERE Codigo IS NULL OR LTRIM(RTRIM(Codigo)) = ''");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Pecas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Estoque",
                table: "Pecas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Pecas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_Pecas_Codigo",
                table: "Pecas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pecas_Nome",
                table: "Pecas",
                column: "Nome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pecas_Codigo",
                table: "Pecas");

            migrationBuilder.DropIndex(
                name: "IX_Pecas_Nome",
                table: "Pecas");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Pecas");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Pecas");

            migrationBuilder.DropColumn(
                name: "Estoque",
                table: "Pecas");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Pecas");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Pecas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Pecas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Preco",
                table: "Pecas",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.RenameColumn(
                name: "Preco",
                table: "Pecas",
                newName: "Valor");
        }
    }
}
