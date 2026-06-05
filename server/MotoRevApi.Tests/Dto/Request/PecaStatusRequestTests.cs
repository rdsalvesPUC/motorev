using System.ComponentModel.DataAnnotations;
using MotoRevApi.Dto.Request;
using MotoRevApi.Enums;
using Xunit;

namespace MotoRevApi.Tests.Dto.Request;

public class PecaStatusRequestTests
{
    [Theory]
    [InlineData(StatusCadastro.Ativo)]
    [InlineData(StatusCadastro.Inativo)]
    public void Validacao_DeveAceitarStatusValido(StatusCadastro status)
    {
        var request = new PecaStatusRequest(status);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validacao_DeveRejeitarStatusAusente()
    {
        var request = new PecaStatusRequest(null);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(PecaStatusRequest.Status));
    }

    private static List<ValidationResult> Validate(PecaStatusRequest request)
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
