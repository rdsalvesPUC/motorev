using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class newmain2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RevisoesPadrao_ModelosMotos_ModeloMotoId",
                table: "RevisoesPadrao");

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
                name: "Logradouro",
                table: "Concessionarias");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Concessionarias");

            migrationBuilder.DropColumn(
                name: "Uf",
                table: "Concessionarias");

            migrationBuilder.RenameColumn(
                name: "ModeloMotoId",
                table: "RevisoesPadrao",
                newName: "LinhaId");

            migrationBuilder.RenameIndex(
                name: "IX_RevisoesPadrao_ModeloMotoId",
                table: "RevisoesPadrao",
                newName: "IX_RevisoesPadrao_LinhaId");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RevisoesPadrao_ConcessionariaId_LinhaId_Ordem",
                table: "RevisoesPadrao",
                columns: new[] { "ConcessionariaId", "LinhaId", "Ordem" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RevisoesPadrao_Linhas_LinhaId",
                table: "RevisoesPadrao",
                column: "LinhaId",
                principalTable: "Linhas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RevisoesPadrao_Linhas_LinhaId",
                table: "RevisoesPadrao");

            migrationBuilder.DropIndex(
                name: "IX_RevisoesPadrao_ConcessionariaId_LinhaId_Ordem",
                table: "RevisoesPadrao");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "LinhaId",
                table: "RevisoesPadrao",
                newName: "ModeloMotoId");

            migrationBuilder.RenameIndex(
                name: "IX_RevisoesPadrao_LinhaId",
                table: "RevisoesPadrao",
                newName: "IX_RevisoesPadrao_ModeloMotoId");

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
                name: "Uf",
                table: "Concessionarias",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_RevisoesPadrao_ModelosMotos_ModeloMotoId",
                table: "RevisoesPadrao",
                column: "ModeloMotoId",
                principalTable: "ModelosMotos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
