using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIndiceAgendamentosDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_LojaId_DataAgendada_Status",
                table: "Agendamentos",
                columns: new[] { "LojaId", "DataAgendada", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Agendamentos_LojaId_DataAgendada_Status",
                table: "Agendamentos");
        }
    }
}
