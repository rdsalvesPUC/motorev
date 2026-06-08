using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace MotoRevApi.Tests.Integration;

public class UploadEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly List<string> _uploadedFiles = new();

    public void Dispose()
    {
        _factory.Dispose();

        // Limpar arquivos temporários criados nos testes
        foreach (var fileUrl in _uploadedFiles)
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // Ignora erros na limpeza
                }
            }
        }
    }

    [Fact]
    public async Task UploadImage_DeveRetornarOkEFileUrl_QuandoImagemForValida()
    {
        // Arrange
        var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent();
        
        // Simular um arquivo PNG de 4 bytes
        var fileContent = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        content.Add(fileContent, "file", "test_avatar.png");

        // Act
        var response = await client.PostAsync("/api/Upload", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Url);
        Assert.StartsWith("/uploads/", result.Url);

        // Rastrear para limpeza posterior
        _uploadedFiles.Add(result.Url);

        // Verificar se o arquivo foi realmente criado no disco
        var fileName = Path.GetFileName(result.Url);
        var expectedPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
        Assert.True(File.Exists(expectedPath), $"O arquivo deveria existir no caminho: {expectedPath}");
    }

    [Fact]
    public async Task UploadImage_DeveRetornarBadRequest_QuandoArquivoForVazio()
    {
        // Arrange
        var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent();
        
        // Simular arquivo vazio (0 bytes)
        var fileContent = new ByteArrayContent(Array.Empty<byte>());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fileContent, "file", "empty.jpg");

        // Act
        var response = await client.PostAsync("/api/Upload", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadImage_DeveRetornarBadRequest_QuandoExtensaoForProibida()
    {
        // Arrange
        var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent();
        
        // Simular arquivo de texto
        var fileContent = new ByteArrayContent("dummy text content"u8.ToArray());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        content.Add(fileContent, "file", "malicious_script.txt");

        // Act
        var response = await client.PostAsync("/api/Upload", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private record UploadResponse(string Url);
}
