using System.ComponentModel.DataAnnotations;
using MotoRevApi.Dto.Request;
using Xunit;

namespace MotoRevApi.Tests.Dto.Request;

public class PecaRequestTests
{
    [Fact]
    public void Validacao_DeveAceitarRequestValido()
    {
        var request = new PecaRequest("Filtro de oleo", "Descricao valida", 10.99m);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validacao_DeveAceitarDescricaoNula()
    {
        var request = new PecaRequest("Filtro de oleo", null, 10.99m);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validacao_DeveAceitarDescricaoVazia()
    {
        var request = new PecaRequest("Filtro de oleo", string.Empty, 10.99m);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validacao_DeveAceitarDescricaoCom1024Caracteres()
    {
        var request = new PecaRequest("Filtro de oleo", new string('A', 1024), 10.99m);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validacao_DeveRejeitarNomeAusente()
    {
        var request = new PecaRequest(null!, "Descricao valida", 10.99m);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Nome));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validacao_DeveRejeitarNomeVazioOuComEspacos(string nome)
    {
        var request = new PecaRequest(nome, "Descricao valida", 10.99m);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Nome));
    }

    [Fact]
    public void Validacao_DeveRejeitarNomeComMenosDe3Caracteres()
    {
        var request = new PecaRequest("AB", "Descricao valida", 10.99m);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Nome));
    }

    [Fact]
    public void Validacao_DeveRejeitarDescricaoComMaisDe1024Caracteres()
    {
        var request = new PecaRequest("Filtro de oleo", new string('A', 1025), 10.99m);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Descricao));
    }

    [Fact]
    public void Validacao_DeveRejeitarValorAusente()
    {
        var request = new PecaRequest("Filtro de oleo", "Descricao valida", null);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Valor));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validacao_DeveRejeitarValorZeroOuNegativo(int valor)
    {
        var request = new PecaRequest("Filtro de oleo", "Descricao valida", valor);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Valor));
    }

    [Fact]
    public void Validacao_DeveRejeitarValorComMaisDe2CasasDecimais()
    {
        var request = new PecaRequest("Filtro de oleo", "Descricao valida", 10.999m);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaRequest.Valor));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(101)]
    [InlineData(1099)]
    public void Validacao_DeveAceitarValorComAte2CasasDecimais(int centavos)
    {
        var valor = centavos / 100m;
        var request = new PecaRequest("Filtro de oleo", "Descricao valida", valor);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
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
