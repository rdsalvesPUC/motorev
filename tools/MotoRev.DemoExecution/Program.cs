using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Enums;
using MotoRevApi.Model;

var options = DemoOptions.Parse(args);
var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
var connectionString = options.ConnectionString
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? ReadDefaultConnectionString(repoRoot)
    ?? throw new InvalidOperationException("Connection string não encontrada.");
var stateFilePath = options.StateFilePath
    ?? Path.Combine(repoRoot, "server", "MotoRevApi", "demo-execucao-state.json");
var dataReferencia = options.Data ?? DateTime.Today;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
};

await using var context = new AppDbContext(
    new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connectionString)
        .Options);

if (options.ListOnly)
{
    await ListarCandidatosAsync(context, dataReferencia, cts.Token);
    return 0;
}

var agendamento = await BuscarAgendamentoAsync(context, options, cts.Token);
if (agendamento == null)
{
    Console.Error.WriteLine("Nenhum agendamento encontrado para a demo.");
    Console.Error.WriteLine($"Cliente: {options.ClienteNome}");
    Console.Error.WriteLine($"Concessionária: {options.ConcessionariaEmail}");
    Console.Error.WriteLine($"Data: {dataReferencia:yyyy-MM-dd}");
    Console.Error.WriteLine();
    await ListarCandidatosAsync(context, dataReferencia, cts.Token);
    return 1;
}

var itens = ObterItens(agendamento.RevisaoMoto).ToList();
Console.WriteLine($"Agendamento #{agendamento.Id} encontrado.");
Console.WriteLine($"Cliente: {agendamento.RevisaoMoto.Moto.Cliente.Nome}");
Console.WriteLine($"Revisão: {agendamento.RevisaoMoto.Nome} (#{agendamento.RevisaoMoto.Id})");
Console.WriteLine($"Itens de execução: {itens.Count}");
Console.WriteLine($"Arquivo de estado: {stateFilePath}");

agendamento.Status = StatusAgendamento.EmExecucao;
agendamento.RevisaoMoto.Status = "Em Execução";
agendamento.AtualizadoEm = DateTime.UtcNow;
await context.SaveChangesAsync(cts.Token);

if (itens.Count == 0)
{
    Console.WriteLine("A revisão não possui peças ou serviços. Marcando como concluída.");
}

for (var index = 0; index < itens.Count; index++)
{
    EscreverEstado(stateFilePath, agendamento.RevisaoMotoId, itens, index);
    Console.WriteLine($"Em execução: {itens[index].Descricao}");

    await Task.Delay(TimeSpan.FromSeconds(options.IntervalSeconds), cts.Token);

    EscreverEstado(stateFilePath, agendamento.RevisaoMotoId, itens, index + 1);
    Console.WriteLine($"Concluído: {itens[index].Descricao}");
}

agendamento.Status = StatusAgendamento.Concluida;
agendamento.RevisaoMoto.Status = "Concluida";
agendamento.AtualizadoEm = DateTime.UtcNow;
await context.SaveChangesAsync(cts.Token);

EscreverEstado(stateFilePath, agendamento.RevisaoMotoId, itens, itens.Count);
Console.WriteLine("Demo finalizada. Revisão marcada como concluída.");
return 0;

static async Task<Agendamento?> BuscarAgendamentoAsync(
    AppDbContext context,
    DemoOptions options,
    CancellationToken cancellationToken)
{
    if (options.AgendamentoId is not null)
    {
        return await QueryAgendamentosComIncludes(context)
            .FirstOrDefaultAsync(a => a.Id == options.AgendamentoId.Value, cancellationToken);
    }

    var hoje = (options.Data ?? DateTime.Today).Date;
    var amanha = hoje.AddDays(1);
    var normalizedEmail = options.ConcessionariaEmail.ToUpperInvariant();
    var clienteNormalizado = Normalizar(options.ClienteNome);

    var candidatos = await QueryAgendamentosComIncludes(context)
        .Where(a =>
            a.DataAgendada >= hoje &&
            a.DataAgendada < amanha &&
            (a.Status == StatusAgendamento.Agendada || a.Status == StatusAgendamento.EmExecucao))
        .OrderBy(a => a.DataAgendada)
        .ThenBy(a => a.CriadoEm)
        .ThenBy(a => a.Id)
        .ToListAsync(cancellationToken);

    return candidatos.FirstOrDefault(a =>
        EmailConcessionariaCorresponde(a, normalizedEmail) &&
        Normalizar(a.RevisaoMoto.Moto.Cliente.Nome).Contains(clienteNormalizado));
}

static IQueryable<Agendamento> QueryAgendamentosComIncludes(AppDbContext context)
{
    return context.Agendamentos
        .Include(a => a.Loja)
            .ThenInclude(l => l.Concessionaria)
                .ThenInclude(c => c.Usuario)
        .Include(a => a.RevisaoMoto)
            .ThenInclude(r => r.Moto)
                .ThenInclude(m => m.Cliente)
        .Include(a => a.RevisaoMoto)
            .ThenInclude(r => r.Moto)
                .ThenInclude(m => m.ModeloMoto)
        .Include(a => a.RevisaoMoto)
            .ThenInclude(r => r.RevisaoPadrao)
                .ThenInclude(rp => rp.Servicos)
                    .ThenInclude(rps => rps.Servico)
        .Include(a => a.RevisaoMoto)
            .ThenInclude(r => r.RevisaoPadrao)
                .ThenInclude(rp => rp.Pecas)
                    .ThenInclude(rpp => rpp.Peca)
        .AsSplitQuery();
}

static async Task ListarCandidatosAsync(AppDbContext context, DateTime data, CancellationToken cancellationToken)
{
    var inicio = data.Date;
    var fim = inicio.AddDays(1);

    var candidatos = await QueryAgendamentosComIncludes(context)
        .Where(a =>
            a.DataAgendada >= inicio &&
            a.DataAgendada < fim)
        .OrderBy(a => a.DataAgendada)
        .ThenBy(a => a.CriadoEm)
        .ThenBy(a => a.Id)
        .ToListAsync(cancellationToken);

    Console.WriteLine($"Agendamentos em {inicio:yyyy-MM-dd}: {candidatos.Count}");

    if (candidatos.Count == 0)
    {
        return;
    }

    foreach (var item in candidatos)
    {
        var email = item.Loja.Concessionaria.Usuario.Email
            ?? item.Loja.Concessionaria.Usuario.NormalizedEmail
            ?? "-";
        var totalItens = item.RevisaoMoto.RevisaoPadrao.Pecas.Count + item.RevisaoMoto.RevisaoPadrao.Servicos.Count;
        Console.WriteLine(
            $"#{item.Id} | {item.Status} | {item.DataAgendada:yyyy-MM-dd HH:mm} | " +
            $"{item.RevisaoMoto.Moto.Cliente.Nome} | {email} | " +
            $"Rev #{item.RevisaoMotoId} - {item.RevisaoMoto.Nome} | itens: {totalItens}");
    }
}

static bool EmailConcessionariaCorresponde(Agendamento agendamento, string normalizedEmail)
{
    var usuario = agendamento.Loja.Concessionaria.Usuario;
    return string.Equals(usuario.NormalizedEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(usuario.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase);
}

static string Normalizar(string value)
{
    var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
    var chars = normalized
        .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
        .ToArray();

    return new string(chars).ToLowerInvariant().Trim();
}

static IEnumerable<DemoItem> ObterItens(RevisaoMoto revisao)
{
    foreach (var peca in revisao.RevisaoPadrao.Pecas.OrderBy(p => p.Peca.Nome).ThenBy(p => p.PecaId))
    {
        yield return new DemoItem(
            $"peca-{peca.PecaId}",
            $"Peça: {peca.Peca.Nome}");
    }

    foreach (var servico in revisao.RevisaoPadrao.Servicos.OrderBy(s => s.Servico.Nome).ThenBy(s => s.ServicoId))
    {
        yield return new DemoItem(
            $"servico-{servico.ServicoId}",
            $"Serviço: {servico.Servico.Nome}");
    }
}

static void EscreverEstado(string stateFilePath, int revisaoMotoId, IReadOnlyList<DemoItem> itens, int itensConcluidos)
{
    var statuses = new Dictionary<string, string>();

    for (var index = 0; index < itens.Count; index++)
    {
        statuses[itens[index].Key] = index < itensConcluidos
            ? "concluido"
            : index == itensConcluidos
                ? "em_execucao"
                : "pendente";
    }

    var state = new DemoExecutionState(
        new Dictionary<string, DemoRevisionExecutionState>
        {
            [revisaoMotoId.ToString()] = new(statuses)
        });

    Directory.CreateDirectory(Path.GetDirectoryName(stateFilePath)!);
    var tempFilePath = $"{stateFilePath}.tmp";
    File.WriteAllText(
        tempFilePath,
        JsonSerializer.Serialize(
            state,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));

    File.Move(tempFilePath, stateFilePath, true);
}

static string FindRepoRoot(string startDirectory)
{
    var directory = new DirectoryInfo(startDirectory);
    while (directory != null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "MotoRev.sln")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new InvalidOperationException("Não foi possível encontrar a raiz do repositório.");
}

static string? ReadDefaultConnectionString(string repoRoot)
{
    var appsettingsPath = Path.Combine(repoRoot, "server", "MotoRevApi", "appsettings.Development.json");
    if (!File.Exists(appsettingsPath))
    {
        return null;
    }

    using var document = JsonDocument.Parse(File.ReadAllText(appsettingsPath));
    return document.RootElement
        .GetProperty("ConnectionStrings")
        .GetProperty("DefaultConnection")
        .GetString();
}

sealed record DemoItem(string Key, string Descricao);

sealed record DemoExecutionState(Dictionary<string, DemoRevisionExecutionState> Revisoes);

sealed record DemoRevisionExecutionState(Dictionary<string, string> Itens);

sealed class DemoOptions
{
    public string ClienteNome { get; private init; } = "Ana Pereira";
    public string ConcessionariaEmail { get; private init; } = "centro.sul.seed@motorev.local";
    public int IntervalSeconds { get; private init; } = 30;
    public int? AgendamentoId { get; private init; }
    public DateTime? Data { get; private init; }
    public bool ListOnly { get; private init; }
    public string? StateFilePath { get; private init; }
    public string? ConnectionString { get; private init; }

    public static DemoOptions Parse(string[] args)
    {
        var options = new DemoOptions();

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            var value = index + 1 < args.Length ? args[index + 1] : null;

            switch (arg)
            {
                case "--cliente" when value != null:
                    options = options.WithCliente(value);
                    index++;
                    break;
                case "--concessionaria-email" when value != null:
                    options = options.WithConcessionariaEmail(value);
                    index++;
                    break;
                case "--interval-seconds" when value != null && int.TryParse(value, out var seconds):
                    options = options.WithIntervalSeconds(Math.Max(1, seconds));
                    index++;
                    break;
                case "--agendamento-id" when value != null && int.TryParse(value, out var agendamentoId):
                    options = options.WithAgendamentoId(agendamentoId);
                    index++;
                    break;
                case "--date" when value != null && DateTime.TryParse(value, out var data):
                    options = options.WithData(data.Date);
                    index++;
                    break;
                case "--list":
                    options = options.WithListOnly();
                    break;
                case "--state-file" when value != null:
                    options = options.WithStateFilePath(value);
                    index++;
                    break;
                case "--connection" when value != null:
                    options = options.WithConnectionString(value);
                    index++;
                    break;
            }
        }

        return options;
    }

    private DemoOptions WithCliente(string value) => new()
    {
        ClienteNome = value,
        ConcessionariaEmail = ConcessionariaEmail,
        IntervalSeconds = IntervalSeconds,
        AgendamentoId = AgendamentoId,
        Data = Data,
        ListOnly = ListOnly,
        StateFilePath = StateFilePath,
        ConnectionString = ConnectionString
    };

    private DemoOptions WithConcessionariaEmail(string value) => new()
    {
        ClienteNome = ClienteNome,
        ConcessionariaEmail = value,
        IntervalSeconds = IntervalSeconds,
        AgendamentoId = AgendamentoId,
        Data = Data,
        ListOnly = ListOnly,
        StateFilePath = StateFilePath,
        ConnectionString = ConnectionString
    };

    private DemoOptions WithIntervalSeconds(int value) => new()
    {
        ClienteNome = ClienteNome,
        ConcessionariaEmail = ConcessionariaEmail,
        IntervalSeconds = value,
        AgendamentoId = AgendamentoId,
        Data = Data,
        ListOnly = ListOnly,
        StateFilePath = StateFilePath,
        ConnectionString = ConnectionString
    };

    private DemoOptions WithAgendamentoId(int value) => new()
    {
        ClienteNome = ClienteNome,
        ConcessionariaEmail = ConcessionariaEmail,
        IntervalSeconds = IntervalSeconds,
        AgendamentoId = value,
        Data = Data,
        ListOnly = ListOnly,
        StateFilePath = StateFilePath,
        ConnectionString = ConnectionString
    };

    private DemoOptions WithData(DateTime value) => new()
    {
        ClienteNome = ClienteNome,
        ConcessionariaEmail = ConcessionariaEmail,
        IntervalSeconds = IntervalSeconds,
        AgendamentoId = AgendamentoId,
        Data = value,
        ListOnly = ListOnly,
        StateFilePath = StateFilePath,
        ConnectionString = ConnectionString
    };

    private DemoOptions WithListOnly() => new()
    {
        ClienteNome = ClienteNome,
        ConcessionariaEmail = ConcessionariaEmail,
        IntervalSeconds = IntervalSeconds,
        AgendamentoId = AgendamentoId,
        Data = Data,
        ListOnly = true,
        StateFilePath = StateFilePath,
        ConnectionString = ConnectionString
    };

    private DemoOptions WithStateFilePath(string value) => new()
    {
        ClienteNome = ClienteNome,
        ConcessionariaEmail = ConcessionariaEmail,
        IntervalSeconds = IntervalSeconds,
        AgendamentoId = AgendamentoId,
        Data = Data,
        ListOnly = ListOnly,
        StateFilePath = value,
        ConnectionString = ConnectionString
    };

    private DemoOptions WithConnectionString(string value) => new()
    {
        ClienteNome = ClienteNome,
        ConcessionariaEmail = ConcessionariaEmail,
        IntervalSeconds = IntervalSeconds,
        AgendamentoId = AgendamentoId,
        Data = Data,
        ListOnly = ListOnly,
        StateFilePath = StateFilePath,
        ConnectionString = value
    };
}
