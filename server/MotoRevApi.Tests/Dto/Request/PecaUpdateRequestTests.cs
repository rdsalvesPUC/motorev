using System.ComponentModel.DataAnnotations;
using MotoRevApi.Dto.Request;
using MotoRevApi.Enums;
using MotoRevApi.Tests.Factories;
using Xunit;

namespace MotoRevApi.Tests.Dto.Request;

public class PecaUpdateRequestTests
{
    [Fact]
    public void Validacao_DeveAceitarRequestValido()
    {
        var request = PecaTestFactory.CreateValidUpdateRequest();

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Validacao_DeveRejeitarCodigoAusenteOuInvalido(string? codigo)
    {
        var request = PecaTestFactory.CreateValidUpdateRequest(codigo: codigo!);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaUpdateRequest.Codigo));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AB")]
    public void Validacao_DeveRejeitarNomeAusenteOuInvalido(string? nome)
    {
        var request = PecaTestFactory.CreateValidUpdateRequest(nome: nome!);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaUpdateRequest.Nome));
    }

    [Fact]
    public void Validacao_DeveRejeitarCategoriaAusente()
    {
        var request = PecaTestFactory.CreateValidUpdateRequest(categoria: null);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaUpdateRequest.Categoria));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validacao_DeveRejeitarPrecoAusenteZeroOuNegativo(int? preco)
    {
        var request = PecaTestFactory.CreateValidUpdateRequest(preco: preco);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaUpdateRequest.Preco));
    }

    [Fact]
    public void Validacao_DeveRejeitarPrecoComMaisDe2CasasDecimais()
    {
        var request = PecaTestFactory.CreateValidUpdateRequest(preco: 10.999m);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaUpdateRequest.Preco));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(101)]
    [InlineData(1099)]
    public void Validacao_DeveAceitarPrecoComAte2CasasDecimais(int centavos)
    {
        var preco = centavos / 100m;
        var request = PecaTestFactory.CreateValidUpdateRequest(preco: preco);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    public void Validacao_DeveRejeitarEstoqueAusenteOuNegativo(int? estoque)
    {
        var request = PecaTestFactory.CreateValidUpdateRequest(estoque: estoque);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaUpdateRequest.Estoque));
    }

    [Fact]
    public void Validacao_DeveRejeitarStatusAusente()
    {
        var request = PecaTestFactory.CreateValidUpdateRequest(status: null);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaUpdateRequest.Status));
    }

    private static List<ValidationResult> Validate(PecaUpdateRequest request)
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
