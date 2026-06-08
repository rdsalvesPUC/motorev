-- MotoRev seed data for Linhas.
-- Re-runnable: updates existing lines by name or inserts them when missing.
-- Usage example:
-- sqlcmd -S localhost,1433 -d MotoRevDb -U sa -P "SuaSenhaForte123!" -i scripts\seed-linhas.sql

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM [dbo].[Linhas] WHERE [Nome] = N'Passeio')
        UPDATE [dbo].[Linhas]
        SET [Descricao] = N'Motos para uso urbano e estradas',
            [Ativo] = 1
        WHERE [Nome] = N'Passeio';
    ELSE
        INSERT INTO [dbo].[Linhas] ([Nome], [Descricao], [Ativo])
        VALUES (N'Passeio', N'Motos para uso urbano e estradas', 1);

    IF EXISTS (SELECT 1 FROM [dbo].[Linhas] WHERE [Nome] = N'Trail')
        UPDATE [dbo].[Linhas]
        SET [Descricao] = N'Motos para uso misto (asfalto e terra)',
            [Ativo] = 1
        WHERE [Nome] = N'Trail';
    ELSE
        INSERT INTO [dbo].[Linhas] ([Nome], [Descricao], [Ativo])
        VALUES (N'Trail', N'Motos para uso misto (asfalto e terra)', 1);

    IF EXISTS (SELECT 1 FROM [dbo].[Linhas] WHERE [Nome] = N'Esportiva')
        UPDATE [dbo].[Linhas]
        SET [Descricao] = N'Motos de alta performance',
            [Ativo] = 1
        WHERE [Nome] = N'Esportiva';
    ELSE
        INSERT INTO [dbo].[Linhas] ([Nome], [Descricao], [Ativo])
        VALUES (N'Esportiva', N'Motos de alta performance', 1);

    IF EXISTS (SELECT 1 FROM [dbo].[Linhas] WHERE [Nome] = N'Scooter')
        UPDATE [dbo].[Linhas]
        SET [Descricao] = N'Motos automaticas urbanas',
            [Ativo] = 1
        WHERE [Nome] = N'Scooter';
    ELSE
        INSERT INTO [dbo].[Linhas] ([Nome], [Descricao], [Ativo])
        VALUES (N'Scooter', N'Motos automaticas urbanas', 1);

    COMMIT TRANSACTION;

    SELECT [Nome], [Descricao], [Ativo]
    FROM [dbo].[Linhas]
    WHERE [Nome] IN (N'Passeio', N'Trail', N'Esportiva', N'Scooter')
    ORDER BY [Nome];
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
