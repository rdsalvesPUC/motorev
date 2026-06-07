using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class AddConcessionariaMatrizELojas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "Concessionarias",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cep",
                table: "Concessionarias",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "Concessionarias",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cnpj",
                table: "Concessionarias",
                type: "nvarchar(18)",
                maxLength: 18,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Logradouro",
                table: "Concessionarias",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Concessionarias",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Concessionarias",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Concessionarias",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Matriz");

            migrationBuilder.AddColumn<string>(
                name: "Uf",
                table: "Concessionarias",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Lojas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Filial"),
                    Cnpj = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cep = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Logradouro = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Bairro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Uf = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ConcessionariaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lojas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lojas_Concessionarias_ConcessionariaId",
                        column: x => x.ConcessionariaId,
                        principalTable: "Concessionarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("UPDATE Concessionarias SET Cnpj = CONCAT('MIGRADO-', Id) WHERE Cnpj = ''");
            migrationBuilder.Sql("UPDATE Concessionarias SET Telefone = CONCAT('MIGRADO-', Id) WHERE Telefone = ''");

            migrationBuilder.Sql("""
                INSERT INTO Lojas (Nome, Tipo, Cnpj, Telefone, Cep, Logradouro, Numero, Bairro, Cidade, Uf, Ativo, ConcessionariaId)
                SELECT Nome, 'Matriz', Cnpj, Telefone, Cep, Logradouro, Numero, Bairro, Cidade, Uf, CAST(1 AS bit), Id
                FROM Concessionarias
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM Lojas
                    WHERE Lojas.ConcessionariaId = Concessionarias.Id
                      AND Lojas.Tipo = 'Matriz'
                )
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Concessionarias_Cnpj",
                table: "Concessionarias",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lojas_Cnpj",
                table: "Lojas",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lojas_ConcessionariaId",
                table: "Lojas",
                column: "ConcessionariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Lojas_ConcessionariaId_Tipo",
                table: "Lojas",
                columns: new[] { "ConcessionariaId", "Tipo" },
                unique: true,
                filter: "[Tipo] = 'Matriz'");

            migrationBuilder.CreateIndex(
                name: "IX_Lojas_Telefone",
                table: "Lojas",
                column: "Telefone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lojas");

            migrationBuilder.DropIndex(
                name: "IX_Concessionarias_Cnpj",
                table: "Concessionarias");

            migrationBuilder.DropColumn(
                name: "Bairro",
                table: "Concessionarias");

            migrationBuilder.DropColumn(
                name: "Cep",
                table: "Concessionarias");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "Concessionarias");

            migrationBuilder.DropColumn(
                name: "Cnpj",
                table: "Concessionarias");

            migrationBuilder.DropColumn(
                name: "Logradouro",
                table: "Concessionarias");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Concessionarias");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Concessionarias");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Concessionarias");

            migrationBuilder.DropColumn(
                name: "Uf",
                table: "Concessionarias");
        }
    }
}
