-- MotoRev seed data for Clientes, Enderecos and Motos.
-- Re-runnable: removes only customers/users/motorcycles created by this script using SEED-CLI-* user ids.
-- Requires scripts\seed-modelos-motos.sql and scripts\seed-revisoes-padrao.sql to have been executed first.
-- Seed customer password: Cliente@123
-- Usage example:
-- sqlcmd -S localhost,1433 -d MotoRevDb -U sa -P "SuaSenhaForte123!" -i scripts\seed-clientes-motos.sql

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

    DECLARE @Clientes TABLE
    (
        [UserId] nvarchar(450) NOT NULL PRIMARY KEY,
        [Nome] nvarchar(150) NOT NULL,
        [Cpf] nvarchar(11) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Telefone] nvarchar(20) NOT NULL,
        [Cep] nvarchar(8) NOT NULL,
        [Logradouro] nvarchar(150) NOT NULL,
        [Numero] nvarchar(20) NOT NULL,
        [Complemento] nvarchar(100) NULL,
        [Bairro] nvarchar(100) NOT NULL,
        [Cidade] nvarchar(100) NOT NULL,
        [Uf] nvarchar(2) NOT NULL
    );

    DECLARE @Motos TABLE
    (
        [UserId] nvarchar(450) NOT NULL,
        [Placa] nvarchar(20) NOT NULL,
        [Chassi] nvarchar(17) NOT NULL,
        [NomeModelo] nvarchar(100) NOT NULL,
        [Cor] nvarchar(30) NOT NULL,
        [KilometragemAtual] int NOT NULL,
        [DataVenda] datetime2 NOT NULL
    );

    INSERT INTO @Clientes
        ([UserId], [Nome], [Cpf], [Email], [Telefone], [Cep], [Logradouro], [Numero], [Complemento], [Bairro], [Cidade], [Uf])
    VALUES
        (N'SEED-CLI-001', N'Ana Pereira Costa', N'12345678062', N'ana.costa.seed@motorev.local', N'11981010001', N'01311000', N'Avenida Paulista', N'1000', N'Apto 101', N'Bela Vista', N'Sao Paulo', N'SP'),
        (N'SEED-CLI-002', N'Bruno Almeida Santos', N'23456789173', N'bruno.santos.seed@motorev.local', N'21982020002', N'20040002', N'Rua da Assembleia', N'45', NULL, N'Centro', N'Rio de Janeiro', N'RJ'),
        (N'SEED-CLI-003', N'Carla Menezes Rocha', N'34567890256', N'carla.rocha.seed@motorev.local', N'31983030003', N'30140071', N'Avenida Afonso Pena', N'2200', N'Sala 304', N'Funcionarios', N'Belo Horizonte', N'MG'),
        (N'SEED-CLI-004', N'Diego Martins Lima', N'45678901320', N'diego.lima.seed@motorev.local', N'41984040004', N'80010000', N'Rua XV de Novembro', N'88', NULL, N'Centro', N'Curitiba', N'PR'),
        (N'SEED-CLI-005', N'Elisa Fernandes Nunes', N'56789012494', N'elisa.nunes.seed@motorev.local', N'51985050005', N'90010150', N'Rua dos Andradas', N'1444', N'Apto 802', N'Centro Historico', N'Porto Alegre', N'RS'),
        (N'SEED-CLI-006', N'Felipe Gomes Araujo', N'67890123540', N'felipe.araujo.seed@motorev.local', N'71986060006', N'40020000', N'Avenida Sete de Setembro', N'510', NULL, N'Vitoria', N'Salvador', N'BA'),
        (N'SEED-CLI-007', N'Gabriela Torres Silva', N'78901234696', N'gabriela.silva.seed@motorev.local', N'81987070007', N'50030000', N'Rua do Bom Jesus', N'32', N'Casa', N'Recife Antigo', N'Recife', N'PE'),
        (N'SEED-CLI-008', N'Hugo Ribeiro Teixeira', N'89012345723', N'hugo.teixeira.seed@motorev.local', N'85988080008', N'60160150', N'Avenida Santos Dumont', N'1789', NULL, N'Aldeota', N'Fortaleza', N'CE'),
        (N'SEED-CLI-009', N'Isabela Barros Melo', N'90123456851', N'isabela.melo.seed@motorev.local', N'61989090009', N'70040902', N'SCS Quadra 2', N'12', N'Bloco B', N'Asa Sul', N'Brasilia', N'DF'),
        (N'SEED-CLI-010', N'Joao Henrique Lopes', N'91234567954', N'joao.lopes.seed@motorev.local', N'62980100010', N'74015010', N'Avenida Goias', N'650', NULL, N'Setor Central', N'Goiania', N'GO');

    -- DataVenda values are chosen so selected 6-month review cycles land on 2026-06-09.
    INSERT INTO @Motos
        ([UserId], [Placa], [Chassi], [NomeModelo], [Cor], [KilometragemAtual], [DataVenda])
    VALUES
        (N'SEED-CLI-001', N'SCA1A01', N'9C2SEEDCLI0010001', N'CG 160', N'Vermelha', 12450, '2025-12-09'),
        (N'SEED-CLI-001', N'SCA1A02', N'9C2SEEDCLI0010002', N'PCX 160', N'Branca', 3200, '2025-06-09'),
        (N'SEED-CLI-002', N'SCB2B01', N'9C2SEEDCLI0020001', N'Factor 150', N'Preta', 18120, '2024-12-09'),
        (N'SEED-CLI-003', N'SCC3C01', N'9C2SEEDCLI0030001', N'Fazer 250', N'Azul', 8700, '2024-06-09'),
        (N'SEED-CLI-003', N'SCC3C02', N'9C2SEEDCLI0030002', N'NMAX 160', N'Cinza', 4600, '2025-12-09'),
        (N'SEED-CLI-004', N'SCD4D01', N'9C2SEEDCLI0040001', N'NXR 160 Bros', N'Verde', 15100, '2023-12-09'),
        (N'SEED-CLI-005', N'SCE5E01', N'9C2SEEDCLI0050001', N'XRE 300', N'Preta', 22400, '2023-06-09'),
        (N'SEED-CLI-005', N'SCE5E02', N'9C2SEEDCLI0050002', N'Meteor 350', N'Azul', 9300, '2024-06-09'),
        (N'SEED-CLI-006', N'SCF6F01', N'9C2SEEDCLI0060001', N'YZF-R3', N'Azul', 7600, '2022-12-09'),
        (N'SEED-CLI-007', N'SCG7G01', N'9C2SEEDCLI0070001', N'Crosser 150', N'Bege', 19900, '2022-12-09'),
        (N'SEED-CLI-007', N'SCG7G02', N'9C2SEEDCLI0070002', N'ADV 160', N'Cinza', 5200, '2024-06-09'),
        (N'SEED-CLI-008', N'SCH8H01', N'9C2SEEDCLI0080001', N'Ninja 400', N'Verde', 6100, '2025-06-09'),
        (N'SEED-CLI-009', N'SCI9I01', N'9C2SEEDCLI0090001', N'Lander 250', N'Azul', 14300, '2024-12-09'),
        (N'SEED-CLI-009', N'SCI9I02', N'9C2SEEDCLI0090002', N'CB 500F', N'Prata', 11800, '2025-06-09'),
        (N'SEED-CLI-010', N'SCJ0J01', N'9C2SEEDCLI0100001', N'G 310 GS', N'Branca', 6800, '2025-12-09');

    IF EXISTS
    (
        SELECT 1
        FROM @Clientes AS seed
        INNER JOIN [dbo].[Clientes] AS cliente
            ON cliente.[Cpf] = seed.[Cpf]
        WHERE cliente.[UsuarioId] NOT LIKE N'SEED-CLI-%'
    )
        THROW 50002, 'CPF de cliente seed ja existe em cliente nao-seed.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Clientes AS seed
        INNER JOIN [dbo].[AspNetUsers] AS usuario
            ON usuario.[NormalizedUserName] = UPPER(seed.[Email])
        WHERE usuario.[Id] NOT LIKE N'SEED-CLI-%'
    )
        THROW 50003, 'Email de cliente seed ja existe em usuario nao-seed.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Motos AS moto
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[ModelosMotos] AS modelo
            WHERE modelo.[NomeModelo] = moto.[NomeModelo]
              AND modelo.[Ativo] = 1
        )
    )
    BEGIN
        SELECT DISTINCT moto.[NomeModelo] AS [ModeloMotoNaoEncontrado]
        FROM @Motos AS moto
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[ModelosMotos] AS modelo
            WHERE modelo.[NomeModelo] = moto.[NomeModelo]
              AND modelo.[Ativo] = 1
        );

        THROW 50004, 'Modelos de moto obrigatorios nao encontrados. Execute scripts\seed-modelos-motos.sql antes deste script.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @Motos AS moto
        INNER JOIN [dbo].[ModelosMotos] AS modelo
            ON modelo.[NomeModelo] = moto.[NomeModelo]
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[RevisoesPadrao] AS revisao
            WHERE revisao.[LinhaId] = modelo.[LinhaId]
              AND revisao.[Ativo] = 1
        )
    )
    BEGIN
        SELECT DISTINCT moto.[NomeModelo] AS [ModeloSemRevisaoPadrao]
        FROM @Motos AS moto
        INNER JOIN [dbo].[ModelosMotos] AS modelo
            ON modelo.[NomeModelo] = moto.[NomeModelo]
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[RevisoesPadrao] AS revisao
            WHERE revisao.[LinhaId] = modelo.[LinhaId]
              AND revisao.[Ativo] = 1
        );

        THROW 50005, 'Modelos de revisao obrigatorios nao encontrados. Execute scripts\seed-revisoes-padrao.sql antes deste script.', 1;
    END;

    DECLARE @SeedEnderecoIds TABLE ([Id] int NOT NULL PRIMARY KEY);

    INSERT INTO @SeedEnderecoIds ([Id])
    SELECT DISTINCT cliente.[EnderecoId]
    FROM [dbo].[Clientes] AS cliente
    WHERE cliente.[UsuarioId] LIKE N'SEED-CLI-%'
      AND cliente.[EnderecoId] IS NOT NULL;

    DELETE revisao
    FROM [dbo].[RevisoesMotos] AS revisao
    INNER JOIN [dbo].[Motos] AS moto
        ON moto.[Id] = revisao.[MotoId]
    INNER JOIN [dbo].[Clientes] AS cliente
        ON cliente.[Id] = moto.[ClienteId]
    WHERE cliente.[UsuarioId] LIKE N'SEED-CLI-%';

    DELETE FROM [dbo].[Motos]
    WHERE [ClienteId] IN
    (
        SELECT cliente.[Id]
        FROM [dbo].[Clientes] AS cliente
        WHERE cliente.[UsuarioId] LIKE N'SEED-CLI-%'
    );

    DELETE FROM [dbo].[AspNetUserRoles]
    WHERE [UserId] LIKE N'SEED-CLI-%';

    DELETE FROM [dbo].[Clientes]
    WHERE [UsuarioId] LIKE N'SEED-CLI-%';

    DELETE endereco
    FROM [dbo].[Enderecos] AS endereco
    INNER JOIN @SeedEnderecoIds AS seed
        ON seed.[Id] = endereco.[Id]
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[Clientes] AS cliente
        WHERE cliente.[EnderecoId] = endereco.[Id]
    );

    DELETE FROM [dbo].[AspNetUsers]
    WHERE [Id] LIKE N'SEED-CLI-%';

    DECLARE @ClienteRoleId nvarchar(450);

    SELECT @ClienteRoleId = [Id]
    FROM [dbo].[AspNetRoles]
    WHERE [NormalizedName] = N'CLIENTE';

    IF @ClienteRoleId IS NULL
    BEGIN
        SET @ClienteRoleId = CONVERT(nvarchar(36), NEWID());

        INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
        VALUES (@ClienteRoleId, N'Cliente', N'CLIENTE', CONVERT(nvarchar(36), NEWID()));
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
        N'AQAAAAIAAYagAAAAEAVIqZ/9saJn+lcu31I/JLlNk2wRz6TFhqHY28l6NQXAZhxELbAS6dPUKRL9X0viDQ==',
        CONVERT(nvarchar(36), NEWID()),
        CONVERT(nvarchar(36), NEWID()),
        seed.[Telefone],
        1,
        0,
        NULL,
        0,
        0
    FROM @Clientes AS seed;

    INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId])
    SELECT seed.[UserId], @ClienteRoleId
    FROM @Clientes AS seed;

    DECLARE @EnderecoMap TABLE
    (
        [UserId] nvarchar(450) NOT NULL PRIMARY KEY,
        [EnderecoId] int NOT NULL
    );

    MERGE [dbo].[Enderecos] AS destino
    USING @Clientes AS origem
        ON 1 = 0
    WHEN NOT MATCHED THEN
        INSERT ([Cep], [Logradouro], [Numero], [Complemento], [Bairro], [Cidade], [Uf])
        VALUES (origem.[Cep], origem.[Logradouro], origem.[Numero], origem.[Complemento], origem.[Bairro], origem.[Cidade], origem.[Uf])
    OUTPUT origem.[UserId], inserted.[Id]
    INTO @EnderecoMap ([UserId], [EnderecoId]);

    INSERT INTO [dbo].[Clientes] ([Nome], [Cpf], [EnderecoId], [UsuarioId])
    SELECT
        seed.[Nome],
        seed.[Cpf],
        endereco.[EnderecoId],
        seed.[UserId]
    FROM @Clientes AS seed
    INNER JOIN @EnderecoMap AS endereco
        ON endereco.[UserId] = seed.[UserId];

    INSERT INTO [dbo].[Motos]
    (
        [Placa],
        [Chassi],
        [ModeloMotoId],
        [ClienteId],
        [ConcessionariaId],
        [Ativo],
        [Foto],
        [Cor],
        [KilometragemAtual],
        [DataVenda]
    )
    SELECT
        moto.[Placa],
        moto.[Chassi],
        modelo.[Id],
        cliente.[Id],
        NULL,
        1,
        NULL,
        moto.[Cor],
        moto.[KilometragemAtual],
        moto.[DataVenda]
    FROM @Motos AS moto
    INNER JOIN [dbo].[Clientes] AS cliente
        ON cliente.[UsuarioId] = moto.[UserId]
    INNER JOIN [dbo].[ModelosMotos] AS modelo
        ON modelo.[NomeModelo] = moto.[NomeModelo];

    INSERT INTO [dbo].[RevisoesMotos]
    (
        [MotoId],
        [RevisaoPadraoId],
        [Nome],
        [Ordem],
        [Quilometragem],
        [TempoMeses],
        [DataPrevista],
        [Status]
    )
    SELECT
        moto.[Id],
        revisao.[Id],
        revisao.[Nome],
        revisao.[Ordem],
        revisao.[Quilometragem],
        revisao.[TempoMeses],
        DATEADD(MONTH, revisao.[TempoMeses], moto.[DataVenda]),
        N'Planejada'
    FROM [dbo].[Motos] AS moto
    INNER JOIN [dbo].[Clientes] AS cliente
        ON cliente.[Id] = moto.[ClienteId]
    INNER JOIN [dbo].[ModelosMotos] AS modelo
        ON modelo.[Id] = moto.[ModeloMotoId]
    INNER JOIN [dbo].[RevisoesPadrao] AS revisao
        ON revisao.[LinhaId] = modelo.[LinhaId]
       AND revisao.[Ativo] = 1
    WHERE cliente.[UsuarioId] LIKE N'SEED-CLI-%';

    COMMIT TRANSACTION;

    SELECT
        cliente.[Nome] AS [Cliente],
        cliente.[Cpf],
        endereco.[Cidade],
        endereco.[Uf],
        COUNT(DISTINCT moto.[Id]) AS [Motos],
        COUNT(revisao.[Id]) AS [RevisoesPlanejadas]
    FROM [dbo].[Clientes] AS cliente
    INNER JOIN [dbo].[Enderecos] AS endereco
        ON endereco.[Id] = cliente.[EnderecoId]
    LEFT JOIN [dbo].[Motos] AS moto
        ON moto.[ClienteId] = cliente.[Id]
       AND moto.[Ativo] = 1
    LEFT JOIN [dbo].[RevisoesMotos] AS revisao
        ON revisao.[MotoId] = moto.[Id]
    WHERE cliente.[UsuarioId] LIKE N'SEED-CLI-%'
    GROUP BY cliente.[Nome], cliente.[Cpf], endereco.[Cidade], endereco.[Uf]
    ORDER BY cliente.[Nome];
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

