using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace MotoRevApi.Services;

public class DemoExecutionStateService
{
    private const string StateFileName = "demo-execucao-state.json";
    private readonly string _stateFilePath;

    public DemoExecutionStateService(IWebHostEnvironment environment)
    {
        _stateFilePath = Environment.GetEnvironmentVariable("MOTOREV_DEMO_EXECUTION_STATE")
            ?? Path.Combine(environment.ContentRootPath, StateFileName);
    }

    public string? ObterStatusExecucao(int revisaoMotoId, string tipo, int itemId)
    {
        var state = CarregarEstado();
        if (state == null || !state.Revisoes.TryGetValue(revisaoMotoId.ToString(), out var revisao))
        {
            return null;
        }

        return revisao.Itens.TryGetValue($"{tipo}-{itemId}", out var status)
            ? status
            : null;
    }

    private DemoExecutionState? CarregarEstado()
    {
        if (!File.Exists(_stateFilePath))
        {
            return null;
        }

        try
        {
            using var stream = File.OpenRead(_stateFilePath);
            return JsonSerializer.Deserialize<DemoExecutionState>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    private sealed record DemoExecutionState(Dictionary<string, DemoRevisionExecutionState> Revisoes);

    private sealed record DemoRevisionExecutionState(Dictionary<string, string> Itens);
}
