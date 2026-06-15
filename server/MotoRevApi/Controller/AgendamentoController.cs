using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Authorization;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

/// <summary>
/// API controller para gerenciamento de agendamentos de revisões.
/// </summary>
/// <remarks>
/// Centraliza os fluxos de visualização, solicitação, cancelamento, remarcação,
/// confirmação e recusa de agendamentos entre Cliente e Concessionária.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Agendamentos")]
public class AgendamentoController : ControllerBase
{
    private readonly AgendamentoService _agendamentoService;

    /// <summary>
    /// Inicializa uma nova instância do controller de agendamentos.
    /// </summary>
    /// <param name="agendamentoService">Serviço responsável pelas regras de negócio de agendamentos.</param>
    public AgendamentoController(AgendamentoService agendamentoService)
    {
        _agendamentoService = agendamentoService;
    }

    /// <summary>
    /// Lista os agendamentos e revisões disponíveis do cliente autenticado.
    /// </summary>
    /// <remarks>
    /// Retorna apenas revisões visíveis na tela de Agendamentos do Cliente:
    /// revisões dentro da janela de tolerância, solicitações aguardando confirmação,
    /// agendamentos confirmados, revisões em execução e revisões atrasadas ainda reagendáveis.
    /// Revisões planejadas fora da janela, concluídas ou perdidas não são retornadas.
    /// </remarks>
    /// <response code="200">Lista de agendamentos do cliente retornada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário autenticado sem a role 'Cliente'.</response>
    /// <response code="404">Cliente não encontrado para o usuário autenticado.</response>
    [HttpGet("cliente")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(typeof(List<AgendamentoClienteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarAgendamentosCliente()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _agendamentoService.ListarAgendamentosClienteAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Cancela um agendamento confirmado do cliente autenticado.
    /// </summary>
    /// <remarks>
    /// Somente agendamentos com status <c>Agendada</c> podem ser cancelados.
    /// Após o cancelamento, a revisão volta a ser exibida para o Cliente como
    /// <c>Aguardando Agendamento</c>, desde que ainda esteja dentro da janela de tolerância.
    /// </remarks>
    /// <param name="agendamentoId">ID do agendamento a ser cancelado.</param>
    /// <response code="204">Agendamento cancelado com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário autenticado sem a role 'Cliente'.</response>
    /// <response code="404">Agendamento não encontrado ou não pertence ao cliente autenticado.</response>
    /// <response code="422">Agendamento não está em um status permitido para cancelamento.</response>
    [HttpPatch("cliente/{agendamentoId:int}/cancelar")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelarAgendamentoCliente(int agendamentoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.CancelarAgendamentoClienteAsync(agendamentoId, userId);
        return NoContent();
    }

    /// <summary>
    /// Solicita a remarcação de um agendamento confirmado.
    /// </summary>
    /// <remarks>
    /// O agendamento atual precisa estar com status <c>Agendada</c>.
    /// Ao solicitar a remarcação, o agendamento antigo é marcado como <c>Cancelada</c>
    /// e uma nova solicitação é criada como <c>AguardandoConfirmacao</c> para análise da Concessionária.
    /// A nova data precisa respeitar a janela de tolerância da revisão.
    /// </remarks>
    /// <param name="agendamentoId">ID do agendamento confirmado que será remarcado.</param>
    /// <param name="request">Nova data solicitada para a revisão.</param>
    /// <response code="204">Solicitação de remarcação criada com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário autenticado sem a role 'Cliente'.</response>
    /// <response code="404">Agendamento não encontrado ou não pertence ao cliente autenticado.</response>
    /// <response code="422">Agendamento ou data solicitada não respeitam as regras de remarcação.</response>
    [HttpPost("cliente/{agendamentoId:int}/remarcar")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RemarcarAgendamentoCliente(
        int agendamentoId,
        [FromBody] RemarcarAgendamentoRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.RemarcarAgendamentoClienteAsync(agendamentoId, userId, request);
        return NoContent();
    }

    /// <summary>
    /// Solicita o agendamento ou reagendamento de uma revisão do cliente.
    /// </summary>
    /// <remarks>
    /// Este endpoint atende dois fluxos do Cliente:
    /// agendar uma revisão em <c>Aguardando Agendamento</c> ou reagendar uma revisão <c>Atrasada</c>.
    /// A revisão precisa estar dentro da janela de tolerância e a loja selecionada precisa estar ativa.
    /// Quando a revisão está atrasada por um agendamento anterior vencido, o registro antigo é cancelado
    /// antes da criação da nova solicitação.
    /// </remarks>
    /// <param name="revisaoMotoId">ID da revisão planejada da moto do cliente.</param>
    /// <param name="request">Loja e data solicitadas para o agendamento.</param>
    /// <response code="204">Solicitação de agendamento criada com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário autenticado sem a role 'Cliente'.</response>
    /// <response code="404">Revisão ou loja não encontrada.</response>
    /// <response code="422">Revisão indisponível para agendamento ou data fora da janela de tolerância.</response>
    [HttpPost("cliente/revisoes/{revisaoMotoId:int}/agendar")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AgendarRevisaoCliente(
        int revisaoMotoId,
        [FromBody] AgendarRevisaoRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.AgendarRevisaoClienteAsync(revisaoMotoId, userId, request);
        return NoContent();
    }

    /// <summary>
    /// Marca a mensagem de recusa como visualizada pelo cliente.
    /// </summary>
    /// <remarks>
    /// Usado quando o Cliente fecha o alerta de solicitação recusada.
    /// A recusa deixa de ser retornada nas próximas listagens de agendamentos do Cliente.
    /// </remarks>
    /// <param name="agendamentoId">ID do agendamento recusado.</param>
    /// <response code="204">Mensagem de recusa marcada como visualizada.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário autenticado sem a role 'Cliente'.</response>
    /// <response code="404">Agendamento não encontrado ou não pertence ao cliente autenticado.</response>
    /// <response code="422">Agendamento não está com status de recusa.</response>
    [HttpPatch("cliente/{agendamentoId:int}/recusa/visualizar")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> VisualizarRecusaCliente(int agendamentoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.VisualizarRecusaClienteAsync(agendamentoId, userId);
        return NoContent();
    }

    /// <summary>
    /// Lista os agendamentos das lojas da concessionária autenticada.
    /// </summary>
    /// <remarks>
    /// Retorna somente agendamentos vinculados às lojas da Concessionária autenticada
    /// e nos status exibidos na tela da Concessionária:
    /// <c>AguardandoConfirmacao</c>, <c>Agendada</c>, <c>Recusada</c>, <c>EmExecucao</c> e <c>Concluida</c>.
    /// </remarks>
    /// <response code="200">Lista de agendamentos da concessionária retornada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário autenticado sem a role 'Concessionaria'.</response>
    /// <response code="404">Concessionária não encontrada para o usuário autenticado.</response>
    [HttpGet("concessionaria")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(List<AgendamentoConcessionariaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarAgendamentosConcessionaria()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _agendamentoService.ListarAgendamentosConcessionariaAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Aceita uma solicitação de agendamento feita por um cliente.
    /// </summary>
    /// <remarks>
    /// Apenas solicitações com status <c>AguardandoConfirmacao</c> podem ser aceitas.
    /// Ao aceitar, o status passa para <c>Agendada</c> e o Cliente passa a visualizar
    /// a revisão como agendada na data solicitada.
    /// </remarks>
    /// <param name="agendamentoId">ID da solicitação de agendamento.</param>
    /// <response code="204">Solicitação aceita com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário autenticado sem a role 'Concessionaria'.</response>
    /// <response code="404">Agendamento não encontrado ou não pertence à concessionária autenticada.</response>
    /// <response code="422">Agendamento não está aguardando confirmação.</response>
    [HttpPatch("concessionaria/{agendamentoId:int}/aceitar")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AceitarSolicitacaoConcessionaria(int agendamentoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.AceitarSolicitacaoConcessionariaAsync(agendamentoId, userId);
        return NoContent();
    }

    /// <summary>
    /// Recusa uma solicitação de agendamento feita por um cliente.
    /// </summary>
    /// <remarks>
    /// Apenas solicitações com status <c>AguardandoConfirmacao</c> podem ser recusadas.
    /// Ao recusar, o Cliente volta a visualizar a revisão como <c>Aguardando Agendamento</c>
    /// e recebe a mensagem de recusa informada pela Concessionária em um alerta fechável.
    /// </remarks>
    /// <param name="agendamentoId">ID da solicitação de agendamento.</param>
    /// <param name="request">Motivo opcional da recusa exibido ao Cliente.</param>
    /// <response code="204">Solicitação recusada com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário autenticado sem a role 'Concessionaria'.</response>
    /// <response code="404">Agendamento não encontrado ou não pertence à concessionária autenticada.</response>
    /// <response code="422">Agendamento não está aguardando confirmação.</response>
    [HttpPost("concessionaria/{agendamentoId:int}/recusar")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecusarSolicitacaoConcessionaria(
        int agendamentoId,
        [FromBody] RecusarAgendamentoRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.RecusarSolicitacaoConcessionariaAsync(agendamentoId, userId, request);
        return NoContent();
    }
}
