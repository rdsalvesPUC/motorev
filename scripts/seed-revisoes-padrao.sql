-- MotoRev seed data for Modelos de Revisao (RevisoesPadrao).
-- Re-runnable: upserts the standard review plans by Linha + Ordem and recreates their seed service/part links.
-- Requires scripts\seed-pecas-servicos.sql and scripts\seed-linhas.sql to have been executed first.
-- Usage example:
-- sqlcmd -S localhost,1433 -d MotoRevDb -U sa -P "SuaSenhaForte123!" -i scripts\seed-revisoes-padrao.sql

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

    DECLARE @Planos TABLE
    (
        [LinhaNome] nvarchar(100) NOT NULL,
        [Ordem] int NOT NULL,
        [Nome] nvarchar(100) NOT NULL,
        [Quilometragem] int NOT NULL,
        [TempoMeses] int NOT NULL,
        [Ativo] bit NOT NULL,
        PRIMARY KEY ([LinhaNome], [Ordem])
    );

    DECLARE @PlanoServicos TABLE
    (
        [LinhaNome] nvarchar(100) NOT NULL,
        [Ordem] int NOT NULL,
        [ServicoCodigo] nvarchar(20) NOT NULL,
        PRIMARY KEY ([LinhaNome], [Ordem], [ServicoCodigo])
    );

    DECLARE @PlanoPecas TABLE
    (
        [LinhaNome] nvarchar(100) NOT NULL,
        [Ordem] int NOT NULL,
        [PecaCodigo] nvarchar(20) NOT NULL,
        [Quantidade] int NOT NULL,
        PRIMARY KEY ([LinhaNome], [Ordem], [PecaCodigo])
    );

    INSERT INTO @Planos ([LinhaNome], [Ordem], [Nome], [Quilometragem], [TempoMeses], [Ativo])
    VALUES
        (N'Passeio', N'1', N'Primeira revisao', 1000, 6, 1),
        (N'Passeio', N'2', N'Segunda revisao', 5000, 12, 1),
        (N'Passeio', N'3', N'Terceira revisao', 10000, 18, 1),
        (N'Passeio', N'4', N'Quarta revisao', 15000, 24, 1),

        (N'Trail', N'1', N'Primeira revisao', 1000, 6, 1),
        (N'Trail', N'2', N'Segunda revisao', 5000, 12, 1),
        (N'Trail', N'3', N'Terceira revisao', 10000, 18, 1),
        (N'Trail', N'4', N'Quarta revisao', 15000, 24, 1),
        (N'Trail', N'5', N'Quinta revisao', 20000, 30, 1),
        (N'Trail', N'6', N'Sexta revisao', 25000, 36, 1),
        (N'Trail', N'7', N'Setima revisao', 30000, 42, 1),

        (N'Esportiva', N'1', N'Primeira revisao', 1000, 6, 1),
        (N'Esportiva', N'2', N'Segunda revisao', 5000, 12, 1),
        (N'Esportiva', N'3', N'Terceira revisao', 10000, 18, 1),
        (N'Esportiva', N'4', N'Quarta revisao', 15000, 24, 1),
        (N'Esportiva', N'5', N'Quinta revisao', 20000, 30, 1),
        (N'Esportiva', N'6', N'Sexta revisao', 25000, 36, 1),
        (N'Esportiva', N'7', N'Setima revisao', 30000, 42, 1),

        (N'Scooter', N'1', N'Primeira revisao', 1000, 6, 1),
        (N'Scooter', N'2', N'Segunda revisao', 5000, 12, 1),
        (N'Scooter', N'3', N'Terceira revisao', 10000, 18, 1),
        (N'Scooter', N'4', N'Quarta revisao', 15000, 24, 1);

    INSERT INTO @PlanoServicos ([LinhaNome], [Ordem], [ServicoCodigo])
    VALUES
        (N'Passeio', 1, N'SEED-SRV-001'), (N'Passeio', 1, N'SEED-SRV-022'), (N'Passeio', 1, N'SEED-SRV-023'),
        (N'Passeio', 2, N'SEED-SRV-001'), (N'Passeio', 2, N'SEED-SRV-008'), (N'Passeio', 2, N'SEED-SRV-015'),
        (N'Passeio', 3, N'SEED-SRV-001'), (N'Passeio', 3, N'SEED-SRV-002'), (N'Passeio', 3, N'SEED-SRV-003'), (N'Passeio', 3, N'SEED-SRV-027'),
        (N'Passeio', 4, N'SEED-SRV-001'), (N'Passeio', 4, N'SEED-SRV-024'), (N'Passeio', 4, N'SEED-SRV-025'), (N'Passeio', 4, N'SEED-SRV-026'),

        (N'Trail', 1, N'SEED-SRV-001'), (N'Trail', 1, N'SEED-SRV-005'), (N'Trail', 1, N'SEED-SRV-022'),
        (N'Trail', 2, N'SEED-SRV-001'), (N'Trail', 2, N'SEED-SRV-006'), (N'Trail', 2, N'SEED-SRV-008'), (N'Trail', 2, N'SEED-SRV-015'),
        (N'Trail', 3, N'SEED-SRV-001'), (N'Trail', 3, N'SEED-SRV-020'), (N'Trail', 3, N'SEED-SRV-023'),
        (N'Trail', 4, N'SEED-SRV-001'), (N'Trail', 4, N'SEED-SRV-002'), (N'Trail', 4, N'SEED-SRV-003'), (N'Trail', 4, N'SEED-SRV-026'),
        (N'Trail', 5, N'SEED-SRV-001'), (N'Trail', 5, N'SEED-SRV-017'), (N'Trail', 5, N'SEED-SRV-027'),
        (N'Trail', 6, N'SEED-SRV-001'), (N'Trail', 6, N'SEED-SRV-005'), (N'Trail', 6, N'SEED-SRV-029'),
        (N'Trail', 7, N'SEED-SRV-001'), (N'Trail', 7, N'SEED-SRV-007'), (N'Trail', 7, N'SEED-SRV-021'), (N'Trail', 7, N'SEED-SRV-030'),

        (N'Esportiva', 1, N'SEED-SRV-001'), (N'Esportiva', 1, N'SEED-SRV-022'), (N'Esportiva', 1, N'SEED-SRV-023'), (N'Esportiva', 1, N'SEED-SRV-004'),
        (N'Esportiva', 2, N'SEED-SRV-001'), (N'Esportiva', 2, N'SEED-SRV-002'), (N'Esportiva', 2, N'SEED-SRV-008'),
        (N'Esportiva', 3, N'SEED-SRV-001'), (N'Esportiva', 3, N'SEED-SRV-017'), (N'Esportiva', 3, N'SEED-SRV-027'),
        (N'Esportiva', 4, N'SEED-SRV-001'), (N'Esportiva', 4, N'SEED-SRV-024'), (N'Esportiva', 4, N'SEED-SRV-025'), (N'Esportiva', 4, N'SEED-SRV-026'),
        (N'Esportiva', 5, N'SEED-SRV-001'), (N'Esportiva', 5, N'SEED-SRV-005'), (N'Esportiva', 5, N'SEED-SRV-021'),
        (N'Esportiva', 6, N'SEED-SRV-001'), (N'Esportiva', 6, N'SEED-SRV-029'), (N'Esportiva', 6, N'SEED-SRV-030'),
        (N'Esportiva', 7, N'SEED-SRV-001'), (N'Esportiva', 7, N'SEED-SRV-004'), (N'Esportiva', 7, N'SEED-SRV-007'), (N'Esportiva', 7, N'SEED-SRV-026'),

        (N'Scooter', 1, N'SEED-SRV-001'), (N'Scooter', 1, N'SEED-SRV-022'), (N'Scooter', 1, N'SEED-SRV-023'),
        (N'Scooter', 2, N'SEED-SRV-001'), (N'Scooter', 2, N'SEED-SRV-006'), (N'Scooter', 2, N'SEED-SRV-010'),
        (N'Scooter', 3, N'SEED-SRV-001'), (N'Scooter', 3, N'SEED-SRV-004'), (N'Scooter', 3, N'SEED-SRV-027'),
        (N'Scooter', 4, N'SEED-SRV-001'), (N'Scooter', 4, N'SEED-SRV-024'), (N'Scooter', 4, N'SEED-SRV-026');

    INSERT INTO @PlanoPecas ([LinhaNome], [Ordem], [PecaCodigo], [Quantidade])
    VALUES
        (N'Passeio', 1, N'SEED-PEC-001', 1), (N'Passeio', 1, N'SEED-PEC-005', 1), (N'Passeio', 1, N'SEED-PEC-021', 1),
        (N'Passeio', 2, N'SEED-PEC-036', 1), (N'Passeio', 2, N'SEED-PEC-061', 1), (N'Passeio', 2, N'SEED-PEC-065', 1),
        (N'Passeio', 3, N'SEED-PEC-002', 1), (N'Passeio', 3, N'SEED-PEC-041', 1), (N'Passeio', 3, N'SEED-PEC-045', 1), (N'Passeio', 3, N'SEED-PEC-022', 1),
        (N'Passeio', 4, N'SEED-PEC-041', 1), (N'Passeio', 4, N'SEED-PEC-042', 1), (N'Passeio', 4, N'SEED-PEC-049', 1), (N'Passeio', 4, N'SEED-PEC-062', 1),

        (N'Trail', 1, N'SEED-PEC-003', 1), (N'Trail', 1, N'SEED-PEC-016', 1), (N'Trail', 1, N'SEED-PEC-021', 1),
        (N'Trail', 2, N'SEED-PEC-061', 1), (N'Trail', 2, N'SEED-PEC-063', 1), (N'Trail', 2, N'SEED-PEC-065', 1),
        (N'Trail', 3, N'SEED-PEC-007', 1), (N'Trail', 3, N'SEED-PEC-016', 1), (N'Trail', 3, N'SEED-PEC-081', 1),
        (N'Trail', 4, N'SEED-PEC-041', 1), (N'Trail', 4, N'SEED-PEC-042', 1), (N'Trail', 4, N'SEED-PEC-049', 1),
        (N'Trail', 5, N'SEED-PEC-010', 1), (N'Trail', 5, N'SEED-PEC-022', 1), (N'Trail', 5, N'SEED-PEC-036', 1),
        (N'Trail', 6, N'SEED-PEC-067', 1), (N'Trail', 6, N'SEED-PEC-074', 1), (N'Trail', 6, N'SEED-PEC-075', 1),
        (N'Trail', 7, N'SEED-PEC-016', 1), (N'Trail', 7, N'SEED-PEC-049', 1), (N'Trail', 7, N'SEED-PEC-082', 1), (N'Trail', 7, N'SEED-PEC-100', 1),

        (N'Esportiva', 1, N'SEED-PEC-004', 1), (N'Esportiva', 1, N'SEED-PEC-015', 1), (N'Esportiva', 1, N'SEED-PEC-023', 1), (N'Esportiva', 1, N'SEED-PEC-082', 1),
        (N'Esportiva', 2, N'SEED-PEC-043', 1), (N'Esportiva', 2, N'SEED-PEC-050', 1), (N'Esportiva', 2, N'SEED-PEC-062', 1),
        (N'Esportiva', 3, N'SEED-PEC-014', 1), (N'Esportiva', 3, N'SEED-PEC-023', 2), (N'Esportiva', 3, N'SEED-PEC-040', 1),
        (N'Esportiva', 4, N'SEED-PEC-043', 1), (N'Esportiva', 4, N'SEED-PEC-044', 1), (N'Esportiva', 4, N'SEED-PEC-049', 1), (N'Esportiva', 4, N'SEED-PEC-052', 1),
        (N'Esportiva', 5, N'SEED-PEC-018', 1), (N'Esportiva', 5, N'SEED-PEC-084', 1), (N'Esportiva', 5, N'SEED-PEC-089', 1),
        (N'Esportiva', 6, N'SEED-PEC-068', 1), (N'Esportiva', 6, N'SEED-PEC-071', 1), (N'Esportiva', 6, N'SEED-PEC-073', 1), (N'Esportiva', 6, N'SEED-PEC-075', 1),
        (N'Esportiva', 7, N'SEED-PEC-019', 1), (N'Esportiva', 7, N'SEED-PEC-049', 1), (N'Esportiva', 7, N'SEED-PEC-082', 1), (N'Esportiva', 7, N'SEED-PEC-098', 1),

        (N'Scooter', 1, N'SEED-PEC-001', 1), (N'Scooter', 1, N'SEED-PEC-008', 1), (N'Scooter', 1, N'SEED-PEC-022', 1),
        (N'Scooter', 2, N'SEED-PEC-045', 1), (N'Scooter', 2, N'SEED-PEC-081', 1), (N'Scooter', 2, N'SEED-PEC-085', 2),
        (N'Scooter', 3, N'SEED-PEC-010', 1), (N'Scooter', 3, N'SEED-PEC-021', 1), (N'Scooter', 3, N'SEED-PEC-087', 1),
        (N'Scooter', 4, N'SEED-PEC-041', 1), (N'Scooter', 4, N'SEED-PEC-049', 1), (N'Scooter', 4, N'SEED-PEC-086', 1);

    IF EXISTS
    (
        SELECT 1
        FROM @Planos AS plano
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[Linhas] AS linha
            WHERE linha.[Nome] = plano.[LinhaNome]
              AND linha.[Ativo] = 1
        )
    )
    BEGIN
        SELECT DISTINCT plano.[LinhaNome] AS [LinhaNaoEncontradaOuInativa]
        FROM @Planos AS plano
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[Linhas] AS linha
            WHERE linha.[Nome] = plano.[LinhaNome]
              AND linha.[Ativo] = 1
        );

        THROW 50021, 'Linhas obrigatorias nao encontradas ou inativas. Execute scripts\seed-linhas.sql antes deste script.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @PlanoServicos AS planoServico
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[Servicos] AS servico
            WHERE servico.[Codigo] = planoServico.[ServicoCodigo]
              AND servico.[Ativo] = 1
        )
    )
    BEGIN
        SELECT DISTINCT planoServico.[ServicoCodigo] AS [ServicoNaoEncontradoOuInativo]
        FROM @PlanoServicos AS planoServico
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[Servicos] AS servico
            WHERE servico.[Codigo] = planoServico.[ServicoCodigo]
              AND servico.[Ativo] = 1
        );

        THROW 50022, 'Servicos obrigatorios nao encontrados ou inativos. Execute scripts\seed-pecas-servicos.sql antes deste script.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @PlanoPecas AS planoPeca
        WHERE planoPeca.[Quantidade] <= 0
           OR NOT EXISTS
           (
               SELECT 1
               FROM [dbo].[Pecas] AS peca
               WHERE peca.[Codigo] = planoPeca.[PecaCodigo]
                 AND peca.[Status] = N'Ativo'
           )
    )
    BEGIN
        SELECT DISTINCT planoPeca.[PecaCodigo] AS [PecaNaoEncontradaOuInativa]
        FROM @PlanoPecas AS planoPeca
        WHERE planoPeca.[Quantidade] <= 0
           OR NOT EXISTS
           (
               SELECT 1
               FROM [dbo].[Pecas] AS peca
               WHERE peca.[Codigo] = planoPeca.[PecaCodigo]
                 AND peca.[Status] = N'Ativo'
           );

        THROW 50023, 'Pecas obrigatorias nao encontradas, inativas ou com quantidade invalida. Execute scripts\seed-pecas-servicos.sql antes deste script.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @Planos AS plano
        OUTER APPLY
        (
            SELECT COUNT(*) AS [Quantidade]
            FROM @PlanoServicos AS servico
            WHERE servico.[LinhaNome] = plano.[LinhaNome]
              AND servico.[Ordem] = plano.[Ordem]
        ) AS servicos
        OUTER APPLY
        (
            SELECT COUNT(*) AS [Quantidade]
            FROM @PlanoPecas AS peca
            WHERE peca.[LinhaNome] = plano.[LinhaNome]
              AND peca.[Ordem] = plano.[Ordem]
        ) AS pecas
        WHERE servicos.[Quantidade] NOT BETWEEN 2 AND 4
           OR pecas.[Quantidade] NOT BETWEEN 2 AND 4
    )
        THROW 50024, 'Cada revisao seed deve conter entre 2 e 4 servicos e entre 2 e 4 pecas.', 1;

    MERGE [dbo].[RevisoesPadrao] AS destino
    USING
    (
        SELECT
            linha.[Id] AS [LinhaId],
            plano.[Ordem],
            plano.[Nome],
            plano.[Quilometragem],
            plano.[TempoMeses],
            plano.[Ativo]
        FROM @Planos AS plano
        INNER JOIN [dbo].[Linhas] AS linha
            ON linha.[Nome] = plano.[LinhaNome]
    ) AS origem
        ON destino.[LinhaId] = origem.[LinhaId]
       AND destino.[Ordem] = origem.[Ordem]
    WHEN MATCHED THEN
        UPDATE SET
            destino.[Nome] = origem.[Nome],
            destino.[Quilometragem] = origem.[Quilometragem],
            destino.[TempoMeses] = origem.[TempoMeses],
            destino.[Ativo] = origem.[Ativo]
    WHEN NOT MATCHED BY TARGET THEN
        INSERT ([Nome], [Ordem], [Quilometragem], [TempoMeses], [LinhaId], [Ativo])
        VALUES (origem.[Nome], origem.[Ordem], origem.[Quilometragem], origem.[TempoMeses], origem.[LinhaId], origem.[Ativo]);

    DECLARE @RevisaoMap TABLE
    (
        [LinhaNome] nvarchar(100) NOT NULL,
        [Ordem] int NOT NULL,
        [RevisaoPadraoId] int NOT NULL,
        PRIMARY KEY ([LinhaNome], [Ordem])
    );

    INSERT INTO @RevisaoMap ([LinhaNome], [Ordem], [RevisaoPadraoId])
    SELECT plano.[LinhaNome], plano.[Ordem], revisao.[Id]
    FROM @Planos AS plano
    INNER JOIN [dbo].[Linhas] AS linha
        ON linha.[Nome] = plano.[LinhaNome]
    INNER JOIN [dbo].[RevisoesPadrao] AS revisao
        ON revisao.[LinhaId] = linha.[Id]
       AND revisao.[Ordem] = plano.[Ordem];

    DELETE revisaoServico
    FROM [dbo].[RevisaoPadraoServicos] AS revisaoServico
    INNER JOIN @RevisaoMap AS revisao
        ON revisao.[RevisaoPadraoId] = revisaoServico.[RevisaoPadraoId];

    DELETE revisaoPeca
    FROM [dbo].[RevisaoPadraoPecas] AS revisaoPeca
    INNER JOIN @RevisaoMap AS revisao
        ON revisao.[RevisaoPadraoId] = revisaoPeca.[RevisaoPadraoId];

    INSERT INTO [dbo].[RevisaoPadraoServicos] ([RevisaoPadraoId], [ServicoId])
    SELECT revisao.[RevisaoPadraoId], servico.[Id]
    FROM @PlanoServicos AS planoServico
    INNER JOIN @RevisaoMap AS revisao
        ON revisao.[LinhaNome] = planoServico.[LinhaNome]
       AND revisao.[Ordem] = planoServico.[Ordem]
    INNER JOIN [dbo].[Servicos] AS servico
        ON servico.[Codigo] = planoServico.[ServicoCodigo];

    INSERT INTO [dbo].[RevisaoPadraoPecas] ([RevisaoPadraoId], [PecaId], [Quantidade])
    SELECT revisao.[RevisaoPadraoId], peca.[Id], planoPeca.[Quantidade]
    FROM @PlanoPecas AS planoPeca
    INNER JOIN @RevisaoMap AS revisao
        ON revisao.[LinhaNome] = planoPeca.[LinhaNome]
       AND revisao.[Ordem] = planoPeca.[Ordem]
    INNER JOIN [dbo].[Pecas] AS peca
        ON peca.[Codigo] = planoPeca.[PecaCodigo];

    COMMIT TRANSACTION;

    SELECT
        linha.[Nome] AS [Linha],
        COUNT(DISTINCT revisao.[Id]) AS [Revisoes],
        COUNT(DISTINCT revisaoServico.[ServicoId]) AS [ServicosDistintos],
        COUNT(DISTINCT revisaoPeca.[PecaId]) AS [PecasDistintas]
    FROM [dbo].[RevisoesPadrao] AS revisao
    INNER JOIN [dbo].[Linhas] AS linha
        ON linha.[Id] = revisao.[LinhaId]
    LEFT JOIN [dbo].[RevisaoPadraoServicos] AS revisaoServico
        ON revisaoServico.[RevisaoPadraoId] = revisao.[Id]
    LEFT JOIN [dbo].[RevisaoPadraoPecas] AS revisaoPeca
        ON revisaoPeca.[RevisaoPadraoId] = revisao.[Id]
    WHERE linha.[Nome] IN (SELECT DISTINCT [LinhaNome] FROM @Planos)
    GROUP BY linha.[Nome]
    ORDER BY linha.[Nome];
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
