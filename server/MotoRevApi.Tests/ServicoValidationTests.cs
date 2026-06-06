using System.ComponentModel.DataAnnotations;
using MotoRevApi.Dto.Request;
using MotoRevApi.Enums;
using Xunit;

namespace MotoRevApi.Tests;

public class ServicoValidationTests
{
    [Theory]
    [InlineData("S1", false)] // Too short
    [InlineData("serv001", false)] // Lowercase
    [InlineData("SERV_001", false)] // Invalid char _
    [InlineData("SERV-001", true)] // Valid
    [InlineData("S001", true)] // Valid
    [InlineData("VERY-LONG-CODE-OVER-TWENTY-CHARS", false)] // Too long
    public void ServicoRequest_Codigo_Validation(string codigo, bool expectedValid)
    {
        var request = new ServicoRequest(codigo, "Nome Valido", "Descricao Valida", CategoriaServico.Troca, 30, 100);
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        
        var isValid = Validator.TryValidateObject(request, context, results, true);
        
        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public void ServicoRequest_Descricao_MaxLength_Validation()
    {
        var longDesc = new string('a', 501);
        var request = new ServicoRequest("S001", "Nome Valido", longDesc, CategoriaServico.Troca, 30, 100);
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        
        var isValid = Validator.TryValidateObject(request, context, results, true);
        
        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage.Contains("descrição não pode exceder 500 caracteres"));
    }
    
    [Theory]
    [InlineData(0, false)] // Min 1
    [InlineData(481, false)] // Max 480
    [InlineData(30, true)]
    public void ServicoRequest_TempoEstimado_Validation(int tempo, bool expectedValid)
    {
        var request = new ServicoRequest("S001", "Nome", "Desc", CategoriaServico.Troca, tempo, 100);
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        
        var isValid = Validator.TryValidateObject(request, context, results, true);
        
        Assert.Equal(expectedValid, isValid);
    }

    [Theory]
    [InlineData(-1, false)] // Negativo - Inválido
    [InlineData(0, true)]   // Zero - Válido
    [InlineData(100, true)] // Positivo - Válido
    public void ServicoRequest_Custo_Validation(decimal custo, bool expectedValid)
    {
        var request = new ServicoRequest("S001", "Nome", "Desc", CategoriaServico.Troca, 30, custo);
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.Equal(expectedValid, isValid);
    }
}
