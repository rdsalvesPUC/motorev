using System.ComponentModel.DataAnnotations;
using MotoRevApi.Dto.Request;
using Xunit;

namespace MotoRevApi.Tests.Dto.Request;

public class UpdateConcessionariaRequestTests
{
    [Fact]
    public void Validacao_DeveAceitarRequestValido()
    {
        var request = new UpdateConcessionariaRequest("Concessionaria Centro", "contato@concessionaria.com");

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validacao_DeveRejeitarNomeAusente()
    {
        var request = new UpdateConcessionariaRequest(null!, "contato@concessionaria.com");

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(UpdateConcessionariaRequest.Nome));
    }

    [Fact]
    public void Validacao_DeveRejeitarEmailAusente()
    {
        var request = new UpdateConcessionariaRequest("Concessionaria Centro", null!);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(UpdateConcessionariaRequest.Email));
    }

    [Theory]
    [InlineData("contato")]
    [InlineData("contato@concessionaria")]
    [InlineData("contato.concessionaria.com")]
    [InlineData("contato@")]
    public void Validacao_DeveRejeitarEmailInvalido(string email)
    {
        var request = new UpdateConcessionariaRequest("Concessionaria Centro", email);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(UpdateConcessionariaRequest.Email));
    }

    private static List<ValidationResult> Validate(UpdateConcessionariaRequest request)
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
