using System.ComponentModel.DataAnnotations;
using MotoRevApi.Dto.Request;
using MotoRevApi.Enums;
using Xunit;

namespace MotoRevApi.Tests.Dto.Request;

public class PecaRequestTests
{
    [Fact]
    public void Validacao_DeveAceitarRequestValido()
    {
        var request = new PecaRequest("P001", "Filtro de oleo", CategoriaPeca.Filtros, 10.99m, 25);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validacao_DeveRejeitarCodigoAusente()
    {
        var request = new PecaRequest(null!, "Filtro de oleo", CategoriaPeca.Filtros, 10.99m, 25);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Codigo));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Validacao_DeveRejeitarCodigoInvalido(string codigo)
    {
        var request = new PecaRequest(codigo, "Filtro de oleo", CategoriaPeca.Filtros, 10.99m, 25);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Codigo));
    }

    [Fact]
    public void Validacao_DeveRejeitarNomeAusente()
    {
        var request = new PecaRequest("P001", null!, CategoriaPeca.Filtros, 10.99m, 25);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Nome));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AB")]
    public void Validacao_DeveRejeitarNomeInvalido(string nome)
    {
        var request = new PecaRequest("P001", nome, CategoriaPeca.Filtros, 10.99m, 25);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Nome));
    }

    [Fact]
    public void Validacao_DeveRejeitarCategoriaAusente()
    {
        var request = new PecaRequest("P001", "Filtro de oleo", null, 10.99m, 25);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Categoria));
    }

    [Fact]
    public void Validacao_DeveRejeitarPrecoAusente()
    {
        var request = new PecaRequest("P001", "Filtro de oleo", CategoriaPeca.Filtros, null, 25);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Preco));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validacao_DeveRejeitarPrecoZeroOuNegativo(int preco)
    {
        var request = new PecaRequest("P001", "Filtro de oleo", CategoriaPeca.Filtros, preco, 25);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Preco));
    }

    [Fact]
    public void Validacao_DeveRejeitarPrecoComMaisDe2CasasDecimais()
    {
        var request = new PecaRequest("P001", "Filtro de oleo", CategoriaPeca.Filtros, 10.999m, 25);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Preco));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(101)]
    [InlineData(1099)]
    public void Validacao_DeveAceitarPrecoComAte2CasasDecimais(int centavos)
    {
        var preco = centavos / 100m;
        var request = new PecaRequest("P001", "Filtro de oleo", CategoriaPeca.Filtros, preco, 25);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validacao_DeveRejeitarEstoqueAusente()
    {
        var request = new PecaRequest("P001", "Filtro de oleo", CategoriaPeca.Filtros, 10.99m, null);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Estoque));
    }

    [Fact]
    public void Validacao_DeveRejeitarEstoqueNegativo()
    {
        var request = new PecaRequest("P001", "Filtro de oleo", CategoriaPeca.Filtros, 10.99m, -1);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Estoque));
    }

    private static List<ValidationResult> Validate(PecaRequest request)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);
        Validator.TryValidateObject(request, validationContext, validationResults, true);
        return validationResults;
    }

    private static void AssertHasErrorFor(IEnumerable<ValidationResult> validationResults, string memberName)
    {
        Assert.Contains(validationResults, result => result.MemberNames.Contains(memberName));
    }
}
