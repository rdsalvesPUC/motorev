-- MotoRev seed data for Concessionarias and Lojas.
-- Re-runnable: removes only dealerships/users/stores created by this script using SEED-CON-* user ids.
-- Seed dealership password: Concessionaria@123
-- Usage example:
-- sqlcmd -S localhost,1433 -d MotoRevDb -U sa -P "SuaSenhaForte123!" -i scripts\seed-concessionarias.sql

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

    DECLARE @Concessionarias TABLE
    (
        [UserId] nvarchar(450) NOT NULL PRIMARY KEY,
        [Nome] nvarchar(150) NOT NULL,
        [Cnpj] nvarchar(18) NOT NULL,
        [Telefone] nvarchar(20) NOT NULL,
        [Email] nvarchar(256) NOT NULL
    );

    DECLARE @Lojas TABLE
    (
        [UserId] nvarchar(450) NOT NULL,
        [Nome] nvarchar(150) NOT NULL,
        [Tipo] nvarchar(20) NOT NULL,
        [Cnpj] nvarchar(18) NOT NULL,
        [Telefone] nvarchar(20) NOT NULL,
        [Cep] nvarchar(9) NOT NULL,
        [Logradouro] nvarchar(150) NOT NULL,
        [Numero] nvarchar(20) NOT NULL,
        [Bairro] nvarchar(100) NOT NULL,
        [Cidade] nvarchar(100) NOT NULL,
        [Uf] nvarchar(2) NOT NULL,
        [Foto] nvarchar(max) NULL
    );

    INSERT INTO @Concessionarias
        ([UserId], [Nome], [Cnpj], [Telefone], [Email])
    VALUES
        (N'SEED-CON-001', N'MotoRev Centro Sul', N'11222333000181', N'4133301001', N'centro.sul.seed@motorev.local'),
        (N'SEED-CON-002', N'MotoRev Paulista', N'22333444000181', N'1133302002', N'paulista.seed@motorev.local'),
        (N'SEED-CON-003', N'MotoRev Litoral Norte', N'33444555000181', N'2133303003', N'litoral.norte.seed@motorev.local');

    INSERT INTO @Lojas
        ([UserId], [Nome], [Tipo], [Cnpj], [Telefone], [Cep], [Logradouro], [Numero], [Bairro], [Cidade], [Uf], [Foto])
    VALUES
        (N'SEED-CON-001', N'MotoRev Centro Sul Matriz', N'Matriz', N'11222333000181', N'4133301001', N'80010-000', N'Rua XV de Novembro', N'1200', N'Centro', N'Curitiba', N'PR', NULL),
        (N'SEED-CON-001', N'MotoRev Centro Sul Batel', N'Filial', N'11222333000262', N'4133301002', N'80420-090', N'Avenida do Batel', N'780', N'Batel', N'Curitiba', N'PR', NULL),
        (N'SEED-CON-001', N'MotoRev Centro Sul Cabral', N'Filial', N'11222333000343', N'4133301003', N'80035-000', N'Avenida Munhoz da Rocha', N'450', N'Cabral', N'Curitiba', N'PR', NULL),
        (N'SEED-CON-001', N'MotoRev Centro Sul Portao', N'Filial', N'11222333000424', N'4133301004', N'81070-000', N'Rua Itacolomi', N'310', N'Portao', N'Curitiba', N'PR', NULL),
        (N'SEED-CON-001', N'MotoRev Centro Sul Pinhais', N'Filial', N'11222333000505', N'4133301005', N'83323-400', N'Avenida Irai', N'1450', N'Weissopolis', N'Pinhais', N'PR', NULL),

        (N'SEED-CON-002', N'MotoRev Paulista Matriz', N'Matriz', N'22333444000181', N'1133302002', N'01311-000', N'Avenida Paulista', N'1578', N'Bela Vista', N'Sao Paulo', N'SP', NULL),
        (N'SEED-CON-002', N'MotoRev Paulista Pinheiros', N'Filial', N'22333444000262', N'1133302003', N'05422-000', N'Rua dos Pinheiros', N'920', N'Pinheiros', N'Sao Paulo', N'SP', NULL),
        (N'SEED-CON-002', N'MotoRev Paulista Moema', N'Filial', N'22333444000343', N'1133302004', N'04077-020', N'Avenida Ibirapuera', N'2100', N'Moema', N'Sao Paulo', N'SP', NULL),
        (N'SEED-CON-002', N'MotoRev Paulista Tatuape', N'Filial', N'22333444000424', N'1133302005', N'03311-000', N'Rua Tuiuti', N'640', N'Tatuape', N'Sao Paulo', N'SP', NULL),
        (N'SEED-CON-002', N'MotoRev Paulista Osasco', N'Filial', N'22333444000505', N'1133302006', N'06020-010', N'Avenida dos Autonomistas', N'1800', N'Centro', N'Osasco', N'SP', NULL),

        (N'SEED-CON-003', N'MotoRev Litoral Norte Matriz', N'Matriz', N'33444555000181', N'2133303003', N'20040-002', N'Rua da Assembleia', N'90', N'Centro', N'Rio de Janeiro', N'RJ', NULL),
        (N'SEED-CON-003', N'MotoRev Litoral Norte Niteroi', N'Filial', N'33444555000262', N'2133303004', N'24020-125', N'Rua Sao Pedro', N'160', N'Centro', N'Niteroi', N'RJ', NULL),
        (N'SEED-CON-003', N'MotoRev Litoral Norte Barra', N'Filial', N'33444555000343', N'2133303005', N'22640-102', N'Avenida das Americas', N'5000', N'Barra da Tijuca', N'Rio de Janeiro', N'RJ', NULL),
        (N'SEED-CON-003', N'MotoRev Litoral Norte Cabo Frio', N'Filial', N'33444555000424', N'2233303006', N'28907-000', N'Avenida Teixeira e Souza', N'950', N'Braga', N'Cabo Frio', N'RJ', NULL),
        (N'SEED-CON-003', N'MotoRev Litoral Norte Petropolis', N'Filial', N'33444555000505', N'2433303007', N'25620-031', N'Rua do Imperador', N'420', N'Centro', N'Petropolis', N'RJ', NULL);

    IF EXISTS
    (
        SELECT 1
        FROM @Concessionarias AS seed
        INNER JOIN [dbo].[AspNetUsers] AS usuario
            ON usuario.[NormalizedUserName] = UPPER(seed.[Email])
        WHERE usuario.[Id] NOT LIKE N'SEED-CON-%'
    )
        THROW 50011, 'Email de concessionaria seed ja existe em usuario nao-seed.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Lojas
        GROUP BY [UserId]
        HAVING COUNT(*) <> 5
            OR SUM(CASE WHEN [Tipo] = N'Matriz' THEN 1 ELSE 0 END) <> 1
    )
        THROW 50014, 'Cada concessionaria seed deve ter exatamente 5 lojas e 1 matriz.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Lojas
        GROUP BY [Cnpj]
        HAVING COUNT(*) > 1
    )
        THROW 50015, 'CNPJ duplicado dentro da seed de lojas.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Lojas
        GROUP BY [Telefone]
        HAVING COUNT(*) > 1
    )
        THROW 50016, 'Telefone duplicado dentro da seed de lojas.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Lojas AS seed
        WHERE EXISTS
        (
            SELECT 1
            FROM [dbo].[Concessionarias] AS concessionaria
            WHERE concessionaria.[Cnpj] = seed.[Cnpj]
              AND concessionaria.[UsuarioId] NOT LIKE N'SEED-CON-%'
        )
        OR EXISTS
        (
            SELECT 1
            FROM [dbo].[Lojas] AS loja
            INNER JOIN [dbo].[Concessionarias] AS concessionaria
                ON concessionaria.[Id] = loja.[ConcessionariaId]
            WHERE loja.[Cnpj] = seed.[Cnpj]
              AND concessionaria.[UsuarioId] NOT LIKE N'SEED-CON-%'
        )
    )
        THROW 50012, 'CNPJ de concessionaria seed ja existe em registro nao-seed.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Lojas AS seed
        INNER JOIN [dbo].[Lojas] AS loja
            ON loja.[Telefone] = seed.[Telefone]
        INNER JOIN [dbo].[Concessionarias] AS concessionaria
            ON concessionaria.[Id] = loja.[ConcessionariaId]
        WHERE concessionaria.[UsuarioId] NOT LIKE N'SEED-CON-%'
    )
        THROW 50013, 'Telefone de concessionaria seed ja existe em loja nao-seed.', 1;

    DECLARE @SeedConcessionariaIds TABLE ([Id] int NOT NULL PRIMARY KEY);

    INSERT INTO @SeedConcessionariaIds ([Id])
    SELECT [Id]
    FROM [dbo].[Concessionarias]
    WHERE [UsuarioId] LIKE N'SEED-CON-%';

    UPDATE [dbo].[Motos]
    SET [ConcessionariaId] = NULL
    WHERE [ConcessionariaId] IN (SELECT [Id] FROM @SeedConcessionariaIds);

    DELETE FROM [dbo].[AspNetUserRoles]
    WHERE [UserId] LIKE N'SEED-CON-%';

    DELETE FROM [dbo].[Concessionarias]
    WHERE [UsuarioId] LIKE N'SEED-CON-%';

    DELETE FROM [dbo].[AspNetUsers]
    WHERE [Id] LIKE N'SEED-CON-%';

    DECLARE @ConcessionariaRoleId nvarchar(450);

    SELECT @ConcessionariaRoleId = [Id]
    FROM [dbo].[AspNetRoles]
    WHERE [NormalizedName] = N'CONCESSIONARIA';

    IF @ConcessionariaRoleId IS NULL
    BEGIN
        SET @ConcessionariaRoleId = CONVERT(nvarchar(36), NEWID());

        INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
        VALUES (@ConcessionariaRoleId, N'Concessionaria', N'CONCESSIONARIA', CONVERT(nvarchar(36), NEWID()));
    END;

    INSERT INTO [dbo].[AspNetUsers]
    (
        [Id],
        [RefreshToken],
        [RefreshTokenExpiryTime],
        [UserName],
        [NormalizedUserName],
        [Email],
        [NormalizedEmail],
        [EmailConfirmed],
        [PasswordHash],
        [SecurityStamp],
        [ConcurrencyStamp],
        [PhoneNumber],
        [PhoneNumberConfirmed],
        [TwoFactorEnabled],
        [LockoutEnd],
        [LockoutEnabled],
        [AccessFailedCount]
    )
    SELECT
        seed.[UserId],
        NULL,
        NULL,
        seed.[Email],
        UPPER(seed.[Email]),
        seed.[Email],
        UPPER(seed.[Email]),
        1,
        N'AQAAAAIAAYagAAAAEISdzkU6DSdCsEeiXVAKQTIXSJ1Sdm15ODdS4qxhKiX1AnSPnsjd18SPogiw/7XpoQ==',
        CONVERT(nvarchar(36), NEWID()),
        CONVERT(nvarchar(36), NEWID()),
        seed.[Telefone],
        1,
        0,
        NULL,
        0,
        0
    FROM @Concessionarias AS seed;

    INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId])
    SELECT seed.[UserId], @ConcessionariaRoleId
    FROM @Concessionarias AS seed;

    DECLARE @ConcessionariaMap TABLE
    (
        [UserId] nvarchar(450) NOT NULL PRIMARY KEY,
        [ConcessionariaId] int NOT NULL
    );

    MERGE [dbo].[Concessionarias] AS destino
    USING @Concessionarias AS origem
        ON 1 = 0
    WHEN NOT MATCHED THEN
        INSERT ([Nome], [Cnpj], [Telefone], [Tipo], [UsuarioId])
        VALUES (origem.[Nome], origem.[Cnpj], origem.[Telefone], N'Matriz', origem.[UserId])
    OUTPUT origem.[UserId], inserted.[Id]
    INTO @ConcessionariaMap ([UserId], [ConcessionariaId]);

    INSERT INTO [dbo].[Lojas]
    (
        [Nome],
        [Tipo],
        [Cnpj],
        [Telefone],
        [Cep],
        [Logradouro],
        [Numero],
        [Bairro],
        [Cidade],
        [Uf],
        [Ativo],
        [Foto],
        [ConcessionariaId]
    )
    SELECT
        loja.[Nome],
        loja.[Tipo],
        loja.[Cnpj],
        loja.[Telefone],
        loja.[Cep],
        loja.[Logradouro],
        loja.[Numero],
        loja.[Bairro],
        loja.[Cidade],
        loja.[Uf],
        1,
        loja.[Foto],
        map.[ConcessionariaId]
    FROM @Lojas AS loja
    INNER JOIN @ConcessionariaMap AS map
        ON map.[UserId] = loja.[UserId];

    COMMIT TRANSACTION;

    SELECT
        concessionaria.[Nome] AS [Concessionaria],
        concessionaria.[Cnpj],
        usuario.[Email],
        COUNT(loja.[Id]) AS [Lojas],
        SUM(CASE WHEN loja.[Tipo] = N'Matriz' THEN 1 ELSE 0 END) AS [Matrizes]
    FROM [dbo].[Concessionarias] AS concessionaria
    INNER JOIN [dbo].[AspNetUsers] AS usuario
        ON usuario.[Id] = concessionaria.[UsuarioId]
    INNER JOIN [dbo].[Lojas] AS loja
        ON loja.[ConcessionariaId] = concessionaria.[Id]
    WHERE concessionaria.[UsuarioId] LIKE N'SEED-CON-%'
    GROUP BY concessionaria.[Nome], concessionaria.[Cnpj], usuario.[Email]
    ORDER BY concessionaria.[Nome];
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
