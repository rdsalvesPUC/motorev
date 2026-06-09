using MotoRevApi.Domain.Revisoes;
using MotoRevApi.Enums;
using Xunit;

namespace MotoRevApi.Tests.Domain;

public class RevisaoMotoStatusPolicyTests
{
    private static readonly DateTime Hoje = new(2026, 6, 9);
    private static readonly DateTime DataPrevista = new(2026, 6, 9);

    [Fact]
    public void StatusPossiveis_DeveConterTodosOsStatusDoFluxoDeRevisao()
    {
        var status = Enum.GetValues<StatusRevisaoMoto>();

        Assert.Contains(StatusRevisaoMoto.Planejada, status);
        Assert.Contains(StatusRevisaoMoto.AguardandoAgendamento, status);
        Assert.Contains(StatusRevisaoMoto.AguardandoConfirmacao, status);
        Assert.Contains(StatusRevisaoMoto.Agendada, status);
        Assert.Contains(StatusRevisaoMoto.EmExecucao, status);
        Assert.Contains(StatusRevisaoMoto.Concluida, status);
        Assert.Contains(StatusRevisaoMoto.Atrasada, status);
        Assert.Contains(StatusRevisaoMoto.Perdida, status);
        Assert.Equal(8, status.Length);
    }

    [Theory]
    [InlineData(StatusRevisaoMoto.Planejada, "Planejada")]
    [InlineData(StatusRevisaoMoto.AguardandoAgendamento, "Aguardando Agendamento")]
    [InlineData(StatusRevisaoMoto.AguardandoConfirmacao, "Aguardando Confirmação")]
    [InlineData(StatusRevisaoMoto.Agendada, "Agendada")]
    [InlineData(StatusRevisaoMoto.EmExecucao, "Em Execução")]
    [InlineData(StatusRevisaoMoto.Concluida, "Concluida")]
    [InlineData(StatusRevisaoMoto.Atrasada, "Atrasada")]
    [InlineData(StatusRevisaoMoto.Perdida, "Perdida")]
    public void ToDisplay_DeveRetornarDescricaoCanonica(StatusRevisaoMoto status, string expected)
    {
        Assert.Equal(expected, RevisaoMotoStatusPolicy.ToDisplay(status));
    }

    [Theory]
    [InlineData("Concluída", StatusRevisaoMoto.Concluida)]
    [InlineData("concluida", StatusRevisaoMoto.Concluida)]
    [InlineData("Aguardando Confirmação", StatusRevisaoMoto.AguardandoConfirmacao)]
    [InlineData("aguardando_confirmacao", StatusRevisaoMoto.AguardandoConfirmacao)]
    [InlineData("Em Execução", StatusRevisaoMoto.EmExecucao)]
    public void FromDatabaseValue_DeveNormalizarAcentosEspacosECaixa(string value, StatusRevisaoMoto expected)
    {
        Assert.Equal(expected, RevisaoMotoStatusPolicy.FromDatabaseValue(value));
    }

    [Fact]
    public void ObterStatusEfetivo_DevePreservarConcluidaMesmoAposJanela()
    {
        var status = RevisaoMotoStatusPolicy.ObterStatusEfetivo(
            StatusRevisaoMoto.Concluida,
            DataPrevista,
            dataAgendamento: null,
            hoje: DataPrevista.AddDays(30));

        Assert.Equal(StatusRevisaoMoto.Concluida, status);
    }

    [Theory]
    [InlineData(StatusRevisaoMoto.EmExecucao)]
    [InlineData(StatusRevisaoMoto.AguardandoConfirmacao)]
    public void ObterStatusEfetivo_DevePreservarStatusOperacionais(StatusRevisaoMoto statusPersistido)
    {
        var status = RevisaoMotoStatusPolicy.ObterStatusEfetivo(
            statusPersistido,
            DataPrevista,
            dataAgendamento: Hoje,
            hoje: Hoje);

        Assert.Equal(statusPersistido, status);
    }

    [Fact]
    public void ObterStatusEfetivo_DeveRetornarPlanejadaAntesDaJanela()
    {
        var status = RevisaoMotoStatusPolicy.ObterStatusEfetivo(
            StatusRevisaoMoto.Planejada,
            DataPrevista,
            dataAgendamento: null,
            hoje: DataPrevista.AddDays(-16));

        Assert.Equal(StatusRevisaoMoto.Planejada, status);
    }

    [Fact]
    public void ObterStatusEfetivo_DeveRetornarAguardandoAgendamentoDentroDaJanelaSemData()
    {
        var status = RevisaoMotoStatusPolicy.ObterStatusEfetivo(
            StatusRevisaoMoto.Planejada,
            DataPrevista,
            dataAgendamento: null,
            hoje: Hoje);

        Assert.Equal(StatusRevisaoMoto.AguardandoAgendamento, status);
    }

    [Fact]
    public void ObterStatusEfetivo_DeveRetornarAgendadaQuandoDataAgendamentoAindaNaoPassou()
    {
        var status = RevisaoMotoStatusPolicy.ObterStatusEfetivo(
            StatusRevisaoMoto.Agendada,
            DataPrevista,
            dataAgendamento: Hoje.AddDays(1),
            hoje: Hoje);

        Assert.Equal(StatusRevisaoMoto.Agendada, status);
    }

    [Fact]
    public void ObterStatusEfetivo_DeveRetornarAtrasadaQuandoDataAgendamentoPassouDentroDaJanela()
    {
        var status = RevisaoMotoStatusPolicy.ObterStatusEfetivo(
            StatusRevisaoMoto.Agendada,
            DataPrevista,
            dataAgendamento: Hoje.AddDays(-1),
            hoje: Hoje);

        Assert.Equal(StatusRevisaoMoto.Atrasada, status);
    }

    [Fact]
    public void ObterStatusEfetivo_DeveRetornarPerdidaDepoisDoLimiteDaJanela()
    {
        var status = RevisaoMotoStatusPolicy.ObterStatusEfetivo(
            StatusRevisaoMoto.Planejada,
            DataPrevista,
            dataAgendamento: null,
            hoje: DataPrevista.AddDays(16));

        Assert.Equal(StatusRevisaoMoto.Perdida, status);
    }

    [Theory]
    [InlineData(StatusRevisaoMoto.AguardandoAgendamento, true)]
    [InlineData(StatusRevisaoMoto.Atrasada, true)]
    [InlineData(StatusRevisaoMoto.Agendada, false)]
    [InlineData(StatusRevisaoMoto.Concluida, false)]
    public void PodeSolicitarAgendamento_DevePermitirApenasDisponiveis(StatusRevisaoMoto status, bool expected)
    {
        Assert.Equal(expected, RevisaoMotoStatusPolicy.PodeSolicitarAgendamento(status));
    }

    [Theory]
    [InlineData(StatusRevisaoMoto.Agendada, true)]
    [InlineData(StatusRevisaoMoto.Atrasada, true)]
    [InlineData(StatusRevisaoMoto.AguardandoConfirmacao, false)]
    public void PodeRemarcar_DevePermitirApenasAgendadasOuAtrasadas(StatusRevisaoMoto status, bool expected)
    {
        Assert.Equal(expected, RevisaoMotoStatusPolicy.PodeRemarcar(status));
    }

    [Theory]
    [InlineData(StatusRevisaoMoto.Agendada, true)]
    [InlineData(StatusRevisaoMoto.AguardandoConfirmacao, true)]
    [InlineData(StatusRevisaoMoto.Atrasada, true)]
    [InlineData(StatusRevisaoMoto.Concluida, false)]
    public void PodeCancelar_DevePermitirApenasAgendamentosAtivos(StatusRevisaoMoto status, bool expected)
    {
        Assert.Equal(expected, RevisaoMotoStatusPolicy.PodeCancelar(status));
    }
}
