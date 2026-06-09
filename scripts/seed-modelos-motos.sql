-- MotoRev seed data for ModelosMotos.
-- Re-runnable: updates existing motorcycle models by NomeModelo or inserts them when missing.
-- Requires scripts\seed-linhas.sql to have been executed first.
-- Usage example:
-- sqlcmd -S localhost,1433 -d MotoRevDb -U sa -P "SuaSenhaForte123!" -i scripts\seed-modelos-motos.sql

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

    DECLARE @Modelos TABLE
    (
        [Marca] nvarchar(50) NOT NULL,
        [NomeModelo] nvarchar(100) NOT NULL,
        [Ano] int NOT NULL,
        [Cilindrada] nvarchar(30) NOT NULL,
        [LinhaNome] nvarchar(100) NOT NULL,
        [Ativo] bit NOT NULL
    );

    INSERT INTO @Modelos ([Marca], [NomeModelo], [Ano], [Cilindrada], [LinhaNome], [Ativo])
    VALUES
        (N'Honda', N'CG 160', 2024, N'160cc', N'Passeio', 1),
        (N'Yamaha', N'Factor 150', 2023, N'150cc', N'Passeio', 1),
        (N'Honda', N'Biz 125', 2024, N'125cc', N'Passeio', 1),
        (N'Yamaha', N'Fazer 250', 2024, N'250cc', N'Passeio', 1),
        (N'Haojue', N'DK 160', 2023, N'160cc', N'Passeio', 1),
        (N'Bajaj', N'Dominar 160', 2024, N'160cc', N'Passeio', 1),
        (N'Royal Enfield', N'Meteor 350', 2024, N'350cc', N'Passeio', 1),
        (N'Honda', N'CB 300F', 2024, N'300cc', N'Passeio', 1),
        (N'Yamaha', N'FZ15', 2024, N'150cc', N'Passeio', 1),
        (N'Kawasaki', N'Z400', 2023, N'400cc', N'Passeio', 1),
        (N'Triumph', N'Speed 400', 2024, N'400cc', N'Passeio', 1),
        (N'BMW', N'G 310 R', 2024, N'310cc', N'Passeio', 1),
        (N'Honda', N'CB 500F', 2023, N'500cc', N'Passeio', 1),
        (N'Dafra', N'Next 300', 2023, N'300cc', N'Passeio', 1),
        (N'Bajaj', N'Dominar 400', 2024, N'400cc', N'Passeio', 1),

        (N'Honda', N'NXR 160 Bros', 2024, N'160cc', N'Trail', 1),
        (N'Honda', N'XRE 190', 2024, N'190cc', N'Trail', 1),
        (N'Honda', N'XRE 300', 2023, N'300cc', N'Trail', 1),
        (N'Yamaha', N'Crosser 150', 2024, N'150cc', N'Trail', 1),
        (N'Yamaha', N'Lander 250', 2024, N'250cc', N'Trail', 1),
        (N'Royal Enfield', N'Himalayan 411', 2023, N'411cc', N'Trail', 1),
        (N'BMW', N'G 310 GS', 2024, N'310cc', N'Trail', 1),
        (N'Kawasaki', N'Versys-X 300', 2023, N'300cc', N'Trail', 1),
        (N'Triumph', N'Tiger Sport 660', 2024, N'660cc', N'Trail', 1),
        (N'Suzuki', N'V-Strom 650', 2023, N'650cc', N'Trail', 1),
        (N'Honda', N'Africa Twin CRF1100L', 2024, N'1100cc', N'Trail', 1),
        (N'Yamaha', N'Tenere 700', 2024, N'700cc', N'Trail', 1),
        (N'BMW', N'R 1250 GS', 2023, N'1250cc', N'Trail', 1),
        (N'KTM', N'390 Adventure', 2024, N'390cc', N'Trail', 1),
        (N'Ducati', N'Multistrada V4', 2024, N'1158cc', N'Trail', 1),

        (N'Suzuki', N'GSX-S750', 2024, N'750cc', N'Esportiva', 1),
        (N'Yamaha', N'YZF-R3', 2024, N'321cc', N'Esportiva', 1),
        (N'Kawasaki', N'Ninja 400', 2024, N'400cc', N'Esportiva', 1),
        (N'Honda', N'CBR 650R', 2024, N'650cc', N'Esportiva', 1),
        (N'BMW', N'S 1000 RR', 2024, N'1000cc', N'Esportiva', 1),
        (N'Ducati', N'Panigale V2', 2024, N'955cc', N'Esportiva', 1),
        (N'Triumph', N'Street Triple RS', 2024, N'765cc', N'Esportiva', 1),
        (N'KTM', N'Duke 390', 2024, N'390cc', N'Esportiva', 1),
        (N'Suzuki', N'Hayabusa', 2024, N'1340cc', N'Esportiva', 1),
        (N'Yamaha', N'MT-09', 2024, N'890cc', N'Esportiva', 1),
        (N'Kawasaki', N'Z900', 2024, N'900cc', N'Esportiva', 1),
        (N'Honda', N'CB 1000R', 2023, N'1000cc', N'Esportiva', 1),
        (N'Aprilia', N'RS 660', 2024, N'660cc', N'Esportiva', 1),
        (N'Ducati', N'Monster', 2024, N'937cc', N'Esportiva', 1),
        (N'BMW', N'F 900 R', 2024, N'900cc', N'Esportiva', 1),

        (N'Honda', N'PCX 160', 2024, N'160cc', N'Scooter', 1),
        (N'Yamaha', N'NMAX 160', 2024, N'160cc', N'Scooter', 1),
        (N'Honda', N'ADV 160', 2024, N'160cc', N'Scooter', 1),
        (N'Shineray', N'Urban 150', 2023, N'150cc', N'Scooter', 1),
        (N'BMW', N'C 400 X', 2024, N'400cc', N'Scooter', 1);

    IF EXISTS
    (
        SELECT 1
        FROM @Modelos AS modelo
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[Linhas] AS linha
            WHERE linha.[Nome] = modelo.[LinhaNome]
        )
    )
    BEGIN
        SELECT DISTINCT modelo.[LinhaNome] AS [LinhaNaoEncontrada]
        FROM @Modelos AS modelo
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[Linhas] AS linha
            WHERE linha.[Nome] = modelo.[LinhaNome]
        );

        THROW 50001, 'Linhas obrigatorias nao encontradas. Execute scripts\seed-linhas.sql antes deste script.', 1;
    END;

    MERGE [dbo].[ModelosMotos] AS destino
    USING
    (
        SELECT
            modelo.[Marca],
            modelo.[NomeModelo],
            linha.[Id] AS [LinhaId],
            modelo.[Cilindrada],
            modelo.[Ano],
            modelo.[Ativo]
        FROM @Modelos AS modelo
        INNER JOIN [dbo].[Linhas] AS linha
            ON linha.[Nome] = modelo.[LinhaNome]
    ) AS origem
        ON destino.[NomeModelo] = origem.[NomeModelo]
    WHEN MATCHED THEN
        UPDATE SET
            destino.[Marca] = origem.[Marca],
            destino.[LinhaId] = origem.[LinhaId],
            destino.[Cilindrada] = origem.[Cilindrada],
            destino.[Ano] = origem.[Ano],
            destino.[Ativo] = origem.[Ativo]
    WHEN NOT MATCHED BY TARGET THEN
        INSERT ([NomeModelo], [Marca], [LinhaId], [Cilindrada], [Ano], [Ativo])
        VALUES (origem.[NomeModelo], origem.[Marca], origem.[LinhaId], origem.[Cilindrada], origem.[Ano], origem.[Ativo]);

    COMMIT TRANSACTION;

    SELECT
        modelo.[Marca],
        modelo.[NomeModelo] AS [Modelo],
        modelo.[Ano],
        modelo.[Cilindrada],
        linha.[Nome] AS [Linha],
        CASE modelo.[Ativo] WHEN 1 THEN N'Ativo' ELSE N'Inativo' END AS [Status]
    FROM [dbo].[ModelosMotos] AS modelo
    INNER JOIN [dbo].[Linhas] AS linha
        ON linha.[Id] = modelo.[LinhaId]
    WHERE modelo.[NomeModelo] IN (SELECT [NomeModelo] FROM @Modelos)
    ORDER BY linha.[Nome], modelo.[Marca], modelo.[NomeModelo];
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
