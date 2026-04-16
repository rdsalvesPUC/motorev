using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Dto.Request;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class ClienteController : ControllerBase
{
    private readonly ClienteService _clienteService;

    public ClienteController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterClienteRequest request)
    {
        var response = await _clienteService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
    }
}
