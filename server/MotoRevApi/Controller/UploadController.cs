using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MotoRevApi.Controller;

/// <summary>
/// API controller para upload de arquivos de imagem.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Upload")]
public class UploadController : ControllerBase
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    /// <summary>
    /// Realiza o upload de uma imagem e a armazena localmente no servidor de arquivos estáticos.
    /// </summary>
    /// <remarks>
    /// Aceita arquivos JPG, JPEG, PNG, WEBP e GIF. O arquivo é salvo em <c>wwwroot/uploads/</c>
    /// com um nome único (GUID) e retorna a URL relativa para acesso público.
    /// </remarks>
    /// <param name="file">O arquivo de imagem a ser enviado (multipart/form-data).</param>
    /// <response code="200">Retorna a URL relativa do arquivo enviado.</response>
    /// <response code="400">Se nenhum arquivo for enviado, estiver vazio ou tiver extensão não permitida.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Nenhum arquivo enviado ou arquivo vazio." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Extensão de arquivo não permitida. Apenas JPG, JPEG, PNG, WEBP e GIF são aceitos." });
        }

        // Criar o diretório wwwroot/uploads caso não exista
        var targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        // Gerar um nome de arquivo exclusivo usando Guid
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var targetFilePath = Path.Combine(targetFolder, uniqueFileName);

        // Salvar o arquivo no disco
        await using (var fileStream = new FileStream(targetFilePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Retorna a URL relativa (caminho estático)
        var fileUrl = $"/uploads/{uniqueFileName}";
        return Ok(new { url = fileUrl });
    }
}
