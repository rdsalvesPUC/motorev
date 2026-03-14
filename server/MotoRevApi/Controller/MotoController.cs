using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

[Route("api/[controller]")]
[ApiController]
public class MotoController : ControllerBase
{
    private readonly MotoService _motoService;
    public MotoController(MotoService motoService)
    {
        _motoService = motoService;
    }
    
    [HttpPost("criar")]
    public IActionResult AdicionarMoto(MotoRequest request)
    {
        var response = _motoService.CadastrarMoto(request);
        return CreatedAtAction(nameof(ObterMoto), new { id = response.Id }, response);
    }
    
    [HttpGet("id/{id}")]
    public ActionResult<MotoResponse> ObterMoto(int id)
    {
        var response = _motoService.ObterMoto(id);
        return Ok(response);
    }
    
    [HttpGet("listar")]
    public ActionResult<List<MotoResponse>> ObterMotos()
    {
        var response = _motoService.ListarMotos();
        return Ok(response);
    }
    
    
}