-- MotoRev seed data for Pecas and Servicos.
-- Re-runnable: removes only records created by this script using SEED-* codes.
-- Usage example:
-- sqlcmd -S localhost,1433 -d MotoRevDb -U sa -P "SuaSenhaForte123!" -i scripts\seed-pecas-servicos.sql

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

    DELETE FROM [dbo].[Servicos]
    WHERE [Codigo] LIKE N'SEED-SRV-%';

    DELETE FROM [dbo].[Pecas]
    WHERE [Codigo] LIKE N'SEED-PEC-%';

    INSERT INTO [dbo].[Servicos] ([Codigo], [Nome], [Descricao], [Categoria], [TempoEstimado], [Custo], [Ativo])
    VALUES
        (N'SEED-SRV-001', N'Verificacao geral de seguranca', N'Inspecao completa dos principais itens de seguranca da motocicleta.', 1, 45, 89.90, 1),
        (N'SEED-SRV-002', N'Verificacao de freios dianteiros', N'Avaliacao de pastilhas, disco, fluido e acionamento do freio dianteiro.', 1, 35, 69.90, 1),
        (N'SEED-SRV-003', N'Verificacao de freios traseiros', N'Avaliacao de lonas, pastilhas, disco, fluido e acionamento traseiro.', 1, 35, 69.90, 1),
        (N'SEED-SRV-004', N'Verificacao eletrica completa', N'Teste de bateria, farois, piscas, rele, chicote e sistema de carga.', 1, 60, 129.90, 1),
        (N'SEED-SRV-005', N'Verificacao de suspensao', N'Inspecao de folgas, vazamentos, buchas e fixacoes da suspensao.', 1, 50, 99.90, 1),
        (N'SEED-SRV-006', N'Verificacao de pneus e rodas', N'Checagem de desgaste, pressao, alinhamento visual e empenos aparentes.', 1, 25, 49.90, 1),
        (N'SEED-SRV-007', N'Verificacao pre viagem', N'Checklist preventivo para uso rodoviario e viagens longas.', 1, 70, 149.90, 1),
        (N'SEED-SRV-008', N'Ajuste de corrente', N'Regulagem da folga da corrente e alinhamento basico da roda traseira.', 2, 25, 39.90, 1),
        (N'SEED-SRV-009', N'Ajuste de embreagem', N'Regulagem da folga do cabo e avaliacao do acionamento da embreagem.', 2, 25, 44.90, 1),
        (N'SEED-SRV-010', N'Ajuste de freio traseiro', N'Regulagem de curso e acionamento do freio traseiro quando aplicavel.', 2, 20, 34.90, 1),
        (N'SEED-SRV-011', N'Ajuste de acelerador', N'Regulagem da folga do cabo e retorno do acelerador.', 2, 20, 34.90, 1),
        (N'SEED-SRV-012', N'Ajuste de marcha lenta', N'Regulagem da rotacao de marcha lenta conforme especificacao do modelo.', 2, 20, 39.90, 1),
        (N'SEED-SRV-013', N'Ajuste de suspensao traseira', N'Regulagem de pre-carga e avaliacao basica de comportamento.', 2, 30, 59.90, 1),
        (N'SEED-SRV-014', N'Ajuste de farol', N'Alinhamento do facho do farol para uso urbano e rodoviario.', 2, 15, 29.90, 1),
        (N'SEED-SRV-015', N'Limpeza de corrente', N'Limpeza da relacao com produto apropriado e lubrificacao final.', 3, 35, 54.90, 1),
        (N'SEED-SRV-016', N'Limpeza de carburador', N'Desmontagem parcial, limpeza de gicles e conferencia de passagem de combustivel.', 3, 90, 179.90, 1),
        (N'SEED-SRV-017', N'Limpeza de corpo de injecao', N'Limpeza do corpo de borboleta e verificacao basica de sensores relacionados.', 3, 75, 159.90, 1),
        (N'SEED-SRV-018', N'Limpeza de filtro esportivo', N'Higienizacao e reaplicacao de oleo em filtro lavavel compativel.', 3, 45, 79.90, 1),
        (N'SEED-SRV-019', N'Limpeza de sistema de freio', N'Limpeza de pincas, suportes e componentes externos do conjunto de freio.', 3, 60, 119.90, 1),
        (N'SEED-SRV-020', N'Limpeza tecnica pos trilha', N'Remocao tecnica de residuos em pontos criticos apos uso severo.', 3, 120, 229.90, 1),
        (N'SEED-SRV-021', N'Limpeza de radiador', N'Limpeza externa do radiador e conferencia visual de aletas e mangueiras.', 3, 50, 99.90, 1),
        (N'SEED-SRV-022', N'Troca de oleo do motor', N'Drenagem do oleo usado e abastecimento com oleo novo conforme especificacao.', 4, 35, 59.90, 1),
        (N'SEED-SRV-023', N'Troca de filtro de oleo', N'Substituicao do filtro de oleo e conferencia de vedacao.', 4, 25, 39.90, 1),
        (N'SEED-SRV-024', N'Troca de pastilha dianteira', N'Substituicao das pastilhas dianteiras e assentamento inicial.', 4, 45, 89.90, 1),
        (N'SEED-SRV-025', N'Troca de pastilha traseira', N'Substituicao das pastilhas traseiras e conferencia do acionamento.', 4, 45, 89.90, 1),
        (N'SEED-SRV-026', N'Troca de fluido de freio', N'Sangria e substituicao do fluido de freio conforme especificacao.', 4, 60, 129.90, 1),
        (N'SEED-SRV-027', N'Troca de vela de ignicao', N'Substituicao da vela e conferencia de cabo ou bobina quando acessivel.', 4, 30, 54.90, 1),
        (N'SEED-SRV-028', N'Troca de bateria', N'Remocao da bateria antiga, instalacao da nova e teste de partida.', 4, 25, 49.90, 1),
        (N'SEED-SRV-029', N'Troca de relacao completa', N'Substituicao de corrente, coroa e pinhao com ajuste final.', 4, 120, 239.90, 1),
        (N'SEED-SRV-030', N'Troca de pneu traseiro', N'Substituicao do pneu traseiro com conferencia de montagem e calibragem.', 4, 75, 149.90, 1);

    INSERT INTO [dbo].[Pecas] ([Codigo], [Nome], [Categoria], [Preco], [Estoque], [Status])
    VALUES
        (N'SEED-PEC-001', N'Filtro de oleo compacto rosqueavel', N'Filtros', 34.90, 48, N'Ativo'),
        (N'SEED-PEC-002', N'Filtro de oleo medio rosqueavel', N'Filtros', 42.90, 36, N'Ativo'),
        (N'SEED-PEC-003', N'Filtro de oleo alto fluxo', N'Filtros', 49.90, 24, N'Ativo'),
        (N'SEED-PEC-004', N'Filtro de oleo premium blindado', N'Filtros', 54.90, 20, N'Ativo'),
        (N'SEED-PEC-005', N'Filtro de ar retangular pequeno', N'Filtros', 39.90, 45, N'Ativo'),
        (N'SEED-PEC-006', N'Filtro de ar retangular medio', N'Filtros', 59.90, 32, N'Ativo'),
        (N'SEED-PEC-007', N'Filtro de ar espuma dupla camada', N'Filtros', 44.90, 38, N'Ativo'),
        (N'SEED-PEC-008', N'Filtro de ar papel plissado', N'Filtros', 47.90, 31, N'Ativo'),
        (N'SEED-PEC-009', N'Filtro de combustivel universal', N'Filtros', 18.90, 70, N'Ativo'),
        (N'SEED-PEC-010', N'Filtro de combustivel injetada', N'Filtros', 29.90, 55, N'Ativo'),
        (N'SEED-PEC-011', N'Filtro de ar esportivo 150cc', N'Filtros', 89.90, 16, N'Ativo'),
        (N'SEED-PEC-012', N'Filtro de ar esportivo 250cc', N'Filtros', 119.90, 12, N'Ativo'),
        (N'SEED-PEC-013', N'Filtro de oleo cartucho pequeno', N'Filtros', 32.90, 27, N'Ativo'),
        (N'SEED-PEC-014', N'Filtro de oleo cartucho medio', N'Filtros', 69.90, 14, N'Ativo'),
        (N'SEED-PEC-015', N'Filtro de ar esportivo conico', N'Filtros', 139.90, 8, N'Ativo'),
        (N'SEED-PEC-016', N'Filtro de ar off-road lavavel', N'Filtros', 74.90, 22, N'Ativo'),
        (N'SEED-PEC-017', N'Filtro de oleo alto desempenho', N'Filtros', 46.90, 30, N'Ativo'),
        (N'SEED-PEC-018', N'Filtro de oleo premium cartucho', N'Filtros', 84.90, 10, N'Ativo'),
        (N'SEED-PEC-019', N'Filtro de ar premium lavavel', N'Filtros', 159.90, 6, N'Ativo'),
        (N'SEED-PEC-020', N'Filtro respiro do motor universal', N'Filtros', 24.90, 44, N'Ativo'),
        (N'SEED-PEC-021', N'Vela NGK CR8E', N'Motor', 29.90, 60, N'Ativo'),
        (N'SEED-PEC-022', N'Vela NGK CPR8EA-9', N'Motor', 34.90, 54, N'Ativo'),
        (N'SEED-PEC-023', N'Vela NGK iridium CR8EIX', N'Motor', 89.90, 25, N'Ativo'),
        (N'SEED-PEC-024', N'Junta tampa de valvula borracha', N'Motor', 22.90, 34, N'Ativo'),
        (N'SEED-PEC-025', N'Junta cabecote monocilindro', N'Motor', 39.90, 28, N'Ativo'),
        (N'SEED-PEC-026', N'Kit pistao standard', N'Motor', 219.90, 9, N'Ativo'),
        (N'SEED-PEC-027', N'Kit aneis standard', N'Motor', 74.90, 15, N'Ativo'),
        (N'SEED-PEC-028', N'Biela completa standard', N'Motor', 189.90, 7, N'Ativo'),
        (N'SEED-PEC-029', N'Virabrequim monocilindro', N'Motor', 399.90, 5, N'Ativo'),
        (N'SEED-PEC-030', N'Comando de valvulas simples', N'Motor', 249.90, 8, N'Ativo'),
        (N'SEED-PEC-031', N'Corrente de comando 96 elos', N'Motor', 79.90, 20, N'Ativo'),
        (N'SEED-PEC-032', N'Tensionador corrente comando', N'Motor', 64.90, 18, N'Ativo'),
        (N'SEED-PEC-033', N'Bomba de oleo engrenagem interna', N'Motor', 129.90, 12, N'Ativo'),
        (N'SEED-PEC-034', N'Retentor de bengala par', N'Motor', 42.90, 30, N'Ativo'),
        (N'SEED-PEC-035', N'Cabo de acelerador 1,10m', N'Motor', 34.90, 40, N'Ativo'),
        (N'SEED-PEC-036', N'Cabo de embreagem 1,15m', N'Motor', 36.90, 42, N'Ativo'),
        (N'SEED-PEC-037', N'Cabo de afogador universal', N'Motor', 28.90, 22, N'Ativo'),
        (N'SEED-PEC-038', N'Mangueira combustivel 5mm', N'Motor', 14.90, 80, N'Ativo'),
        (N'SEED-PEC-039', N'Kit reparo carburador 26mm', N'Motor', 59.90, 18, N'Ativo'),
        (N'SEED-PEC-040', N'Bico injetor multiponto', N'Motor', 179.90, 11, N'Ativo'),
        (N'SEED-PEC-041', N'Pastilha freio dianteira organica', N'Freios', 49.90, 50, N'Ativo'),
        (N'SEED-PEC-042', N'Pastilha freio traseira organica', N'Freios', 69.90, 36, N'Ativo'),
        (N'SEED-PEC-043', N'Pastilha freio dianteira sinterizada', N'Freios', 79.90, 28, N'Ativo'),
        (N'SEED-PEC-044', N'Pastilha freio traseira sinterizada', N'Freios', 74.90, 26, N'Ativo'),
        (N'SEED-PEC-045', N'Lona freio traseiro 110mm', N'Freios', 44.90, 40, N'Ativo'),
        (N'SEED-PEC-046', N'Disco freio dianteiro 220mm', N'Freios', 169.90, 15, N'Ativo'),
        (N'SEED-PEC-047', N'Disco freio dianteiro 267mm', N'Freios', 219.90, 10, N'Ativo'),
        (N'SEED-PEC-048', N'Disco freio traseiro 220mm', N'Freios', 249.90, 8, N'Ativo'),
        (N'SEED-PEC-049', N'Fluido de freio DOT 4 500ml', N'Freios', 39.90, 33, N'Ativo'),
        (N'SEED-PEC-050', N'Flexivel freio dianteiro 75cm', N'Freios', 89.90, 17, N'Ativo'),
        (N'SEED-PEC-051', N'Reparo cilindro mestre dianteiro', N'Freios', 64.90, 19, N'Ativo'),
        (N'SEED-PEC-052', N'Reparo pinca freio dianteira', N'Freios', 54.90, 21, N'Ativo'),
        (N'SEED-PEC-053', N'Pinca freio dianteira completa', N'Freios', 289.90, 6, N'Ativo'),
        (N'SEED-PEC-054', N'Pedal de freio articulado', N'Freios', 74.90, 14, N'Ativo'),
        (N'SEED-PEC-055', N'Manete de freio curto', N'Freios', 29.90, 45, N'Ativo'),
        (N'SEED-PEC-056', N'Sensor de freio traseiro', N'Freios', 24.90, 37, N'Ativo'),
        (N'SEED-PEC-057', N'Sapatas freio tambor universal', N'Freios', 59.90, 24, N'Ativo'),
        (N'SEED-PEC-058', N'Reservatorio fluido freio', N'Freios', 49.90, 13, N'Ativo'),
        (N'SEED-PEC-059', N'Parafuso sangrador freio', N'Freios', 12.90, 70, N'Ativo'),
        (N'SEED-PEC-060', N'Kit freio dianteiro completo', N'Freios', 349.90, 5, N'Ativo'),
        (N'SEED-PEC-061', N'Corrente 428H 118L', N'Transmissao', 119.90, 30, N'Ativo'),
        (N'SEED-PEC-062', N'Corrente 520H 106L', N'Transmissao', 189.90, 18, N'Ativo'),
        (N'SEED-PEC-063', N'Coroa 43 dentes passo 428', N'Transmissao', 79.90, 26, N'Ativo'),
        (N'SEED-PEC-064', N'Coroa 45 dentes passo 428', N'Transmissao', 89.90, 22, N'Ativo'),
        (N'SEED-PEC-065', N'Pinhao 14 dentes passo 428', N'Transmissao', 34.90, 38, N'Ativo'),
        (N'SEED-PEC-066', N'Pinhao 15 dentes passo 520', N'Transmissao', 44.90, 24, N'Ativo'),
        (N'SEED-PEC-067', N'Kit relacao passo 428', N'Transmissao', 219.90, 16, N'Ativo'),
        (N'SEED-PEC-068', N'Kit relacao passo 520 leve', N'Transmissao', 289.90, 11, N'Ativo'),
        (N'SEED-PEC-069', N'Kit relacao passo 520 reforcado', N'Transmissao', 329.90, 9, N'Ativo'),
        (N'SEED-PEC-070', N'Cubo embreagem seis molas', N'Transmissao', 159.90, 10, N'Ativo'),
        (N'SEED-PEC-071', N'Discos embreagem kit organico', N'Transmissao', 89.90, 23, N'Ativo'),
        (N'SEED-PEC-072', N'Separadores embreagem kit aco', N'Transmissao', 54.90, 19, N'Ativo'),
        (N'SEED-PEC-073', N'Mola embreagem reforcada kit', N'Transmissao', 39.90, 28, N'Ativo'),
        (N'SEED-PEC-074', N'Rolamento roda dianteira par', N'Transmissao', 49.90, 32, N'Ativo'),
        (N'SEED-PEC-075', N'Rolamento roda traseira par', N'Transmissao', 54.90, 29, N'Ativo'),
        (N'SEED-PEC-076', N'Retentor roda traseira', N'Transmissao', 19.90, 44, N'Ativo'),
        (N'SEED-PEC-077', N'Guia corrente reforcado', N'Transmissao', 69.90, 14, N'Ativo'),
        (N'SEED-PEC-078', N'Esticador corrente par', N'Transmissao', 34.90, 25, N'Ativo'),
        (N'SEED-PEC-079', N'Coxim coroa borracha', N'Transmissao', 59.90, 20, N'Ativo'),
        (N'SEED-PEC-080', N'Eixo roda traseira 17mm', N'Transmissao', 84.90, 12, N'Ativo'),
        (N'SEED-PEC-081', N'Bateria 12V 5Ah selada', N'Eletrica', 189.90, 18, N'Ativo'),
        (N'SEED-PEC-082', N'Bateria 12V 7Ah selada', N'Eletrica', 249.90, 14, N'Ativo'),
        (N'SEED-PEC-083', N'Lampada farol H4 35W', N'Eletrica', 24.90, 60, N'Ativo'),
        (N'SEED-PEC-084', N'Lampada farol LED H4', N'Eletrica', 89.90, 22, N'Ativo'),
        (N'SEED-PEC-085', N'Lampada pisca 12V 10W', N'Eletrica', 8.90, 100, N'Ativo'),
        (N'SEED-PEC-086', N'Rele de partida 12V', N'Eletrica', 54.90, 26, N'Ativo'),
        (N'SEED-PEC-087', N'Rele pisca universal', N'Eletrica', 29.90, 35, N'Ativo'),
        (N'SEED-PEC-088', N'Regulador retificador 12V', N'Eletrica', 119.90, 16, N'Ativo'),
        (N'SEED-PEC-089', N'Estator trifasico', N'Eletrica', 249.90, 8, N'Ativo'),
        (N'SEED-PEC-090', N'Bobina ignicao 12V', N'Eletrica', 79.90, 24, N'Ativo'),
        (N'SEED-PEC-091', N'Cachimbo vela NGK', N'Eletrica', 22.90, 50, N'Ativo'),
        (N'SEED-PEC-092', N'Cabo vela universal', N'Eletrica', 18.90, 45, N'Ativo'),
        (N'SEED-PEC-093', N'Chave ignicao universal', N'Eletrica', 99.90, 15, N'Ativo'),
        (N'SEED-PEC-094', N'Interruptor partida punho direito', N'Eletrica', 64.90, 13, N'Ativo'),
        (N'SEED-PEC-095', N'Interruptor luz punho esquerdo', N'Eletrica', 69.90, 12, N'Ativo'),
        (N'SEED-PEC-096', N'Sensor neutro rosca M10', N'Eletrica', 39.90, 28, N'Ativo'),
        (N'SEED-PEC-097', N'Sensor velocidade magnetico', N'Eletrica', 129.90, 9, N'Ativo'),
        (N'SEED-PEC-098', N'Chicote principal 12 vias', N'Eletrica', 299.90, 6, N'Ativo'),
        (N'SEED-PEC-099', N'Fusivel lamina 10A', N'Eletrica', 4.90, 120, N'Ativo'),
        (N'SEED-PEC-100', N'Tomada USB guidon 12V', N'Eletrica', 79.90, 20, N'Ativo');

    COMMIT TRANSACTION;

    SELECT N'Pecas' AS [Entidade], COUNT(*) AS [Registros]
    FROM [dbo].[Pecas]
    WHERE [Codigo] LIKE N'SEED-PEC-%'
    UNION ALL
    SELECT N'Servicos' AS [Entidade], COUNT(*) AS [Registros]
    FROM [dbo].[Servicos]
    WHERE [Codigo] LIKE N'SEED-SRV-%';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
