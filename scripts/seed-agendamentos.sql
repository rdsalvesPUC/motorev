-- MotoRev seed data for Agendamentos.
-- Re-runnable: resets only appointment data for the seed customer SEED-CLI-001.
-- Requires scripts\seed-clientes-motos.sql and scripts\seed-concessionarias.sql to have been executed first.
-- Validation account: ana.costa.seed@motorev.local / Cliente@123
-- Usage example:
-- sqlcmd -S localhost,1433 -d MotoRevDb -U sa -P "SuaSenhaForte123!" -i scripts\seed-agendamentos.sql

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

    IF OBJECT_ID(N'[dbo].[Agendamentos]', N'U') IS NULL
        THROW 50031, 'Tabela Agendamentos nao encontrada. Execute as migrations antes deste script.', 1;

    DECLARE @Hoje date = CONVERT(date, GETDATE());
    DECLARE @ClienteUserId nvarchar(450) = N'SEED-CLI-001';
    DECLARE @LojaSaoPauloId int;
    DECLARE @LojaCuritibaId int;

    SELECT TOP (1) @LojaSaoPauloId = loja.[Id]
    FROM [dbo].[Lojas] AS loja
    INNER JOIN [dbo].[Concessionarias] AS concessionaria
        ON concessionaria.[Id] = loja.[ConcessionariaId]
    WHERE concessionaria.[UsuarioId] LIKE N'SEED-CON-%'
      AND loja.[Cidade] = N'Sao Paulo'
      AND loja.[Ativo] = 1
    ORDER BY CASE WHEN loja.[Tipo] = N'Matriz' THEN 0 ELSE 1 END, loja.[Id];

    SELECT TOP (1) @LojaCuritibaId = loja.[Id]
    FROM [dbo].[Lojas] AS loja
    INNER JOIN [dbo].[Concessionarias] AS concessionaria
        ON concessionaria.[Id] = loja.[ConcessionariaId]
    WHERE concessionaria.[UsuarioId] LIKE N'SEED-CON-%'
      AND loja.[Cidade] = N'Curitiba'
      AND loja.[Ativo] = 1
    ORDER BY CASE WHEN loja.[Tipo] = N'Matriz' THEN 0 ELSE 1 END, loja.[Id];

    IF @LojaSaoPauloId IS NULL OR @LojaCuritibaId IS NULL
        THROW 50032, 'Lojas seed obrigatorias nao encontradas. Execute scripts\seed-concessionarias.sql antes deste script.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[Clientes] AS cliente
        WHERE cliente.[UsuarioId] = @ClienteUserId
    )
        THROW 50033, 'Cliente seed SEED-CLI-001 nao encontrado. Execute scripts\seed-clientes-motos.sql antes deste script.', 1;

    DECLARE @Cenarios TABLE
    (
        [Placa] nvarchar(20) NOT NULL,
        [Ordem] int NOT NULL,
        [StatusTelaEsperado] nvarchar(40) NOT NULL,
        [StatusRevisao] nvarchar(40) NOT NULL,
        [StatusAgendamento] nvarchar(40) NULL,
        [DataAgendada] date NULL,
        [LojaId] int NULL,
        [CriadoEm] datetime2 NULL,
        PRIMARY KEY ([Placa], [Ordem])
    );

    INSERT INTO @Cenarios
        ([Placa], [Ordem], [StatusTelaEsperado], [StatusRevisao], [StatusAgendamento], [DataAgendada], [LojaId], [CriadoEm])
    VALUES
        -- Status visiveis na tela Agendamentos. Cada cenario usa uma moto diferente
        -- da Ana, e a DataPrevista vem de DataVenda + TempoMeses.
        (N'SCA1A01', 1, N'aguardando_agendamento', N'Planejada', NULL, NULL, NULL, NULL),
        (N'SCA1A02', 1, N'aguardando_confirmacao', N'Planejada', N'AguardandoConfirmacao', DATEADD(DAY, 2, @Hoje), @LojaSaoPauloId, DATEADD(HOUR, -5, CAST(@Hoje AS datetime2))),
        (N'SCA1A03', 1, N'agendada', N'Planejada', N'Agendada', DATEADD(DAY, 7, @Hoje), @LojaSaoPauloId, DATEADD(HOUR, -4, CAST(@Hoje AS datetime2))),
        (N'SCA1A04', 1, N'atrasada', N'Planejada', N'Agendada', DATEADD(DAY, -1, @Hoje), @LojaCuritibaId, DATEADD(HOUR, -3, CAST(@Hoje AS datetime2))),
        (N'SCA1A05', 1, N'em_execucao', N'Planejada', N'EmExecucao', DATEADD(DAY, -1, @Hoje), @LojaSaoPauloId, DATEADD(HOUR, -2, CAST(@Hoje AS datetime2))),

        -- Status de controle que nao devem aparecer como cards.
        (N'SCA1A06', 1, N'planejada_oculta', N'Planejada', NULL, NULL, NULL, NULL),
        (N'SCA1A07', 1, N'perdida_oculta', N'Planejada', NULL, NULL, NULL, NULL),
        (N'SCA1A08', 1, N'concluida_oculta', N'Concluida', N'Concluida', DATEADD(DAY, -1, @Hoje), @LojaSaoPauloId, DATEADD(HOUR, -1, CAST(@Hoje AS datetime2)));

    IF EXISTS
    (
        SELECT 1
        FROM @Cenarios AS cenario
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[RevisoesMotos] AS revisao
            INNER JOIN [dbo].[Motos] AS moto
                ON moto.[Id] = revisao.[MotoId]
            INNER JOIN [dbo].[Clientes] AS cliente
                ON cliente.[Id] = moto.[ClienteId]
            WHERE cliente.[UsuarioId] = @ClienteUserId
              AND moto.[Placa] = cenario.[Placa]
              AND revisao.[Ordem] = cenario.[Ordem]
        )
    )
    BEGIN
        SELECT cenario.[Placa], cenario.[Ordem]
        FROM @Cenarios AS cenario
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[RevisoesMotos] AS revisao
            INNER JOIN [dbo].[Motos] AS moto
                ON moto.[Id] = revisao.[MotoId]
            INNER JOIN [dbo].[Clientes] AS cliente
                ON cliente.[Id] = moto.[ClienteId]
            WHERE cliente.[UsuarioId] = @ClienteUserId
              AND moto.[Placa] = cenario.[Placa]
              AND revisao.[Ordem] = cenario.[Ordem]
        );

        THROW 50034, 'Revisoes seed obrigatorias nao encontradas para montar os cenarios de agendamento.', 1;
    END;

    DELETE agendamento
    FROM [dbo].[Agendamentos] AS agendamento
    INNER JOIN [dbo].[RevisoesMotos] AS revisao
        ON revisao.[Id] = agendamento.[RevisaoMotoId]
    INNER JOIN [dbo].[Motos] AS moto
        ON moto.[Id] = revisao.[MotoId]
    INNER JOIN [dbo].[Clientes] AS cliente
        ON cliente.[Id] = moto.[ClienteId]
    WHERE cliente.[UsuarioId] = @ClienteUserId;

    UPDATE revisao
    SET
        revisao.[Status] = cenario.[StatusRevisao]
    FROM [dbo].[RevisoesMotos] AS revisao
    INNER JOIN [dbo].[Motos] AS moto
        ON moto.[Id] = revisao.[MotoId]
    INNER JOIN [dbo].[Clientes] AS cliente
        ON cliente.[Id] = moto.[ClienteId]
    INNER JOIN @Cenarios AS cenario
        ON cenario.[Placa] = moto.[Placa]
       AND cenario.[Ordem] = revisao.[Ordem]
    WHERE cliente.[UsuarioId] = @ClienteUserId;

    INSERT INTO [dbo].[Agendamentos]
    (
        [RevisaoMotoId],
        [LojaId],
        [DataAgendada],
        [Status],
        [CriadoEm],
        [AtualizadoEm]
    )
    SELECT
        revisao.[Id],
        cenario.[LojaId],
        cenario.[DataAgendada],
        cenario.[StatusAgendamento],
        cenario.[CriadoEm],
        cenario.[CriadoEm]
    FROM @Cenarios AS cenario
    INNER JOIN [dbo].[Motos] AS moto
        ON moto.[Placa] = cenario.[Placa]
    INNER JOIN [dbo].[Clientes] AS cliente
        ON cliente.[Id] = moto.[ClienteId]
       AND cliente.[UsuarioId] = @ClienteUserId
    INNER JOIN [dbo].[RevisoesMotos] AS revisao
        ON revisao.[MotoId] = moto.[Id]
       AND revisao.[Ordem] = cenario.[Ordem]
    WHERE cenario.[StatusAgendamento] IS NOT NULL;

    COMMIT TRANSACTION;

    SELECT
        cliente.[Nome] AS [Cliente],
        usuario.[Email],
        moto.[Placa],
        revisao.[Ordem],
        revisao.[Nome] AS [Revisao],
        cenario.[StatusTelaEsperado],
        revisao.[DataPrevista],
        agendamento.[Status] AS [StatusAgendamento],
        agendamento.[DataAgendada],
        loja.[Nome] AS [Loja]
    FROM @Cenarios AS cenario
    INNER JOIN [dbo].[Motos] AS moto
        ON moto.[Placa] = cenario.[Placa]
    INNER JOIN [dbo].[Clientes] AS cliente
        ON cliente.[Id] = moto.[ClienteId]
       AND cliente.[UsuarioId] = @ClienteUserId
    INNER JOIN [dbo].[AspNetUsers] AS usuario
        ON usuario.[Id] = cliente.[UsuarioId]
    INNER JOIN [dbo].[RevisoesMotos] AS revisao
        ON revisao.[MotoId] = moto.[Id]
       AND revisao.[Ordem] = cenario.[Ordem]
    LEFT JOIN [dbo].[Agendamentos] AS agendamento
        ON agendamento.[RevisaoMotoId] = revisao.[Id]
    LEFT JOIN [dbo].[Lojas] AS loja
        ON loja.[Id] = agendamento.[LojaId]
    ORDER BY
        CASE cenario.[StatusTelaEsperado]
            WHEN N'em_execucao' THEN 1
            WHEN N'aguardando_confirmacao' THEN 2
            WHEN N'agendada' THEN 3
            WHEN N'atrasada' THEN 4
            WHEN N'aguardando_agendamento' THEN 5
            ELSE 6
        END,
        moto.[Placa],
        revisao.[Ordem];
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
