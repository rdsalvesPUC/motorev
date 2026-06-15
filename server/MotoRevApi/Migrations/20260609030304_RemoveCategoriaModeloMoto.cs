using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRevApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoriaModeloMoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[ModelosMotos]', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_ModelosMotos_NomeModelo'
          AND object_id = OBJECT_ID(N'[ModelosMotos]')
    )
    BEGIN
        DROP INDEX [IX_ModelosMotos_NomeModelo] ON [ModelosMotos];
    END

    IF COL_LENGTH(N'[ModelosMotos]', N'Categoria') IS NOT NULL
    BEGIN
        ALTER TABLE [ModelosMotos] DROP COLUMN [Categoria];
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_ModelosMotos_NomeModelo'
          AND object_id = OBJECT_ID(N'[ModelosMotos]')
    )
    AND COL_LENGTH(N'[ModelosMotos]', N'NomeModelo') IS NOT NULL
    AND COL_LENGTH(N'[ModelosMotos]', N'Ativo') IS NOT NULL
    BEGIN
        CREATE UNIQUE INDEX [IX_ModelosMotos_NomeModelo]
            ON [ModelosMotos] ([NomeModelo])
            WHERE [Ativo] = 1;
    END
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[ModelosMotos]', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_ModelosMotos_NomeModelo'
          AND object_id = OBJECT_ID(N'[ModelosMotos]')
    )
    BEGIN
        DROP INDEX [IX_ModelosMotos_NomeModelo] ON [ModelosMotos];
    END

    IF COL_LENGTH(N'[ModelosMotos]', N'Categoria') IS NULL
    BEGIN
        ALTER TABLE [ModelosMotos]
            ADD [Categoria] nvarchar(50) NULL;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_ModelosMotos_NomeModelo'
          AND object_id = OBJECT_ID(N'[ModelosMotos]')
    )
    AND COL_LENGTH(N'[ModelosMotos]', N'NomeModelo') IS NOT NULL
    BEGIN
        CREATE UNIQUE INDEX [IX_ModelosMotos_NomeModelo]
            ON [ModelosMotos] ([NomeModelo]);
    END
END");
        }
    }
}
