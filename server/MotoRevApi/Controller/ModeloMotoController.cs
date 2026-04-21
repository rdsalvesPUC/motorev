using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

[Route("api/[controller]")]
[ApiController]
public class ModeloMotoController : ControllerBase
{
    private readonly ModeloMotoService _modeloMotoService;

    public ModeloMotoController(ModeloMotoService modeloMotoService)
    {
        _modeloMotoService = modeloMotoService;
    }

    [HttpPost("criar")]
    public IActionResult AdicionarModeloMoto([FromBody] ModeloMotoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = _modeloMotoService.CadastrarModeloMoto(request);
        return CreatedAtAction(nameof(ObterModeloMoto), new { id = response.Id }, response);
    }

    [HttpGet("id/{id}")]
    public ActionResult<ModeloMotoResponse> ObterModeloMoto(int id)
    {
        var response = _modeloMotoService.ObterModeloMoto(id);
        if (response == null)
        {
            return NotFound(new { message = "Modelo de moto não encontrado." });
        }
        return Ok(response);
    }

    [HttpGet("listar")]
    public ActionResult<List<ModeloMotoResponse>> ObterModelosMotos()
    {
        var response = _modeloMotoService.ListarModelosMotos();
        return Ok(response);
    }

    [HttpPut("atualizar/{id}")]
    public IActionResult AtualizarModeloMoto(int id, [FromBody] ModeloMotoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = _modeloMotoService.AtualizarModeloMoto(id, request);
        
        if (response == null)
        {
            return NotFound(new { message = "Modelo de moto não encontrado." });
        }

        return Ok(response);
    }

    [HttpPatch("alternar-status/{id}")]
    public IActionResult AlternarStatusModeloMoto(int id)
    {
        var response = _modeloMotoService.AlternarStatus(id);

        if (response == null)
        {
            return NotFound(new { message = "Modelo de moto não encontrado." });
        }

        return Ok(new { message = response.Ativo ? "Modelo ativado com sucesso." : "Modelo inativado com sucesso.", modelo = response });
    }
}
