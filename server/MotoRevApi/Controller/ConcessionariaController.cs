using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Dto.Request;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class ConcessionariaController : ControllerBase
{
    private readonly ConcessionariaService _concessionariaService;

    public ConcessionariaController(ConcessionariaService concessionariaService)
    {
        _concessionariaService = concessionariaService;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterConcessionariaRequest request)
    {
        var response = await _concessionariaService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
    }
}
