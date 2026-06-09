using System.Globalization;
using System.Text;
using MotoRevApi.Enums;

namespace MotoRevApi.Domain.Revisoes;

public static class RevisaoMotoStatusPolicy
{
    public static string ToDisplay(StatusRevisaoMoto status)
    {
        return status switch
        {
            StatusRevisaoMoto.Planejada => "Planejada",
            StatusRevisaoMoto.AguardandoAgendamento => "Aguardando Agendamento",
            StatusRevisaoMoto.AguardandoConfirmacao => "Aguardando Confirmação",
            StatusRevisaoMoto.Agendada => "Agendada",
            StatusRevisaoMoto.EmExecucao => "Em Execução",
            StatusRevisaoMoto.Concluida => "Concluida",
            StatusRevisaoMoto.Atrasada => "Atrasada",
            StatusRevisaoMoto.Perdida => "Perdida",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static StatusRevisaoMoto FromDatabaseValue(string? value)
    {
        return Normalize(value) switch
        {
            "planejada" => StatusRevisaoMoto.Planejada,
            "aguardando_agendamento" => StatusRevisaoMoto.AguardandoAgendamento,
            "aguardando_confirmacao" => StatusRevisaoMoto.AguardandoConfirmacao,
            "agendada" => StatusRevisaoMoto.Agendada,
            "em_execucao" => StatusRevisaoMoto.EmExecucao,
            "concluida" => StatusRevisaoMoto.Concluida,
            "atrasada" => StatusRevisaoMoto.Atrasada,
            "perdida" => StatusRevisaoMoto.Perdida,
            _ => StatusRevisaoMoto.Planejada
        };
    }

    public static StatusRevisaoMoto ObterStatusEfetivo(
        StatusRevisaoMoto statusPersistido,
        DateTime dataPrevista,
        DateTime? dataAgendamento,
        DateTime hoje)
    {
        if (statusPersistido is StatusRevisaoMoto.Concluida
            or StatusRevisaoMoto.EmExecucao
            or StatusRevisaoMoto.AguardandoConfirmacao)
        {
            return statusPersistido;
        }

        var hojeDate = hoje.Date;
        var dataMinima = dataPrevista.Date.AddDays(-15);
        var dataLimite = dataPrevista.Date.AddDays(15);

        if (hojeDate > dataLimite)
        {
            return StatusRevisaoMoto.Perdida;
        }

        if (hojeDate < dataMinima)
        {
            return StatusRevisaoMoto.Planejada;
        }

        if (dataAgendamento.HasValue)
        {
            return hojeDate > dataAgendamento.Value.Date
                ? StatusRevisaoMoto.Atrasada
                : StatusRevisaoMoto.Agendada;
        }

        return statusPersistido == StatusRevisaoMoto.Agendada
            ? StatusRevisaoMoto.Agendada
            : StatusRevisaoMoto.AguardandoAgendamento;
    }

    public static bool PodeSolicitarAgendamento(StatusRevisaoMoto statusEfetivo)
    {
        return statusEfetivo is StatusRevisaoMoto.AguardandoAgendamento or StatusRevisaoMoto.Atrasada;
    }

    public static bool PodeRemarcar(StatusRevisaoMoto statusEfetivo)
    {
        return statusEfetivo is StatusRevisaoMoto.Agendada or StatusRevisaoMoto.Atrasada;
    }

    public static bool PodeCancelar(StatusRevisaoMoto statusEfetivo)
    {
        return statusEfetivo is StatusRevisaoMoto.Agendada
            or StatusRevisaoMoto.AguardandoConfirmacao
            or StatusRevisaoMoto.Atrasada;
    }

    private static string Normalize(string? value)
    {
        var normalized = (value ?? string.Empty).Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "_");
    }
}
