using System.ComponentModel.DataAnnotations;
using MotoRevApi.Dto.Request;
using Xunit;

namespace MotoRevApi.Tests.Dto.Request;

public class RevisaoPadraoUpdateRequestTests
{
    [Fact]
    public void Validacao_DeveAceitarRequestValido()
    {
        var request = CreateValidRequest();

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validacao_DeveAceitarPecasAusentes()
    {
        var request = CreateValidRequest(pecas: null);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validacao_DeveRejeitarNomeAusente()
    {
        var request = CreateValidRequest(nome: null!);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(RevisaoPadraoUpdateRequest.Nome));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validacao_DeveRejeitarOrdemZeroOuNegativa(int ordem)
    {
        var request = CreateValidRequest(ordem: ordem);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(RevisaoPadraoUpdateRequest.Ordem));
    }

    [Fact]
    public void Validacao_DeveRejeitarQuilometragemNegativa()
    {
        var request = CreateValidRequest(quilometragem: -1);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(RevisaoPadraoUpdateRequest.Quilometragem));
    }

    [Fact]
    public void Validacao_DeveRejeitarTempoMesesNegativo()
    {
        var request = CreateValidRequest(tempoMeses: -1);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(RevisaoPadraoUpdateRequest.TempoMeses));
    }

    [Fact]
    public void Validacao_DeveRejeitarServicosAusentes()
    {
        var request = new RevisaoPadraoUpdateRequest(
            "Revisao 1000km",
            1,
            1000,
            6,
            null!);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(RevisaoPadraoUpdateRequest.ServicosIds));
    }

    [Fact]
    public void Validacao_DeveRejeitarServicosVazios()
    {
        var request = CreateValidRequest(servicosIds: new List<int>());

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(RevisaoPadraoUpdateRequest.ServicosIds));
    }

    private static RevisaoPadraoUpdateRequest CreateValidRequest(
        string nome = "Revisao 1000km",
        int ordem = 1,
        int quilometragem = 1000,
        int tempoMeses = 6,
        List<int>? servicosIds = null,
        List<RevisaoPadraoPecaRequest>? pecas = null)
    {
        return new RevisaoPadraoUpdateRequest(
            nome,
            ordem,
            quilometragem,
            tempoMeses,
            servicosIds ?? new List<int> { 1 },
            pecas);
    }

    private static List<ValidationResult> Validate(RevisaoPadraoUpdateRequest request)
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
