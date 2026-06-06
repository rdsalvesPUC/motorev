using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class RecreateMotosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ano",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "Cor",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "Modelo",
                table: "Motos");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Motos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Chassi",
                table: "Motos",
                type: "nvarchar(17)",
                maxLength: 17,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Motos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConcessionariaId",
                table: "Motos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModeloMotoId",
                table: "Motos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Placa",
                table: "Motos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Motos_Chassi",
                table: "Motos",
                column: "Chassi",
                unique: true,
                filter: "[Ativo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Motos_ClienteId",
                table: "Motos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Motos_ConcessionariaId",
                table: "Motos",
                column: "ConcessionariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Motos_ModeloMotoId",
                table: "Motos",
                column: "ModeloMotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Motos_Placa",
                table: "Motos",
                column: "Placa",
                unique: true,
                filter: "[Ativo] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_Motos_Clientes_ClienteId",
                table: "Motos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Motos_Concessionarias_ConcessionariaId",
                table: "Motos",
                column: "ConcessionariaId",
                principalTable: "Concessionarias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Motos_ModelosMotos_ModeloMotoId",
                table: "Motos",
                column: "ModeloMotoId",
                principalTable: "ModelosMotos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Motos_Clientes_ClienteId",
                table: "Motos");

            migrationBuilder.DropForeignKey(
                name: "FK_Motos_Concessionarias_ConcessionariaId",
                table: "Motos");

            migrationBuilder.DropForeignKey(
                name: "FK_Motos_ModelosMotos_ModeloMotoId",
                table: "Motos");

            migrationBuilder.DropIndex(
                name: "IX_Motos_Chassi",
                table: "Motos");

            migrationBuilder.DropIndex(
                name: "IX_Motos_ClienteId",
                table: "Motos");

            migrationBuilder.DropIndex(
                name: "IX_Motos_ConcessionariaId",
                table: "Motos");

            migrationBuilder.DropIndex(
                name: "IX_Motos_ModeloMotoId",
                table: "Motos");

            migrationBuilder.DropIndex(
                name: "IX_Motos_Placa",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "Chassi",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "ConcessionariaId",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "ModeloMotoId",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "Placa",
                table: "Motos");

            migrationBuilder.AddColumn<string>(
                name: "Ano",
                table: "Motos",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cor",
                table: "Motos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modelo",
                table: "Motos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
