using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIndiceStatusPeca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Pecas_Status",
                table: "Pecas",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pecas_Status",
                table: "Pecas");
        }
    }
}
