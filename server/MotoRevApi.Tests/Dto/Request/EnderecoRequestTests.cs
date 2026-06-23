using System.ComponentModel.DataAnnotations;
using MotoRevApi.Dto.Request;
using Xunit;

namespace MotoRevApi.Tests.Dto.Request;

public class EnderecoRequestTests
{
    [Fact]
    public void Validacao_DeveAceitarRequestValido()
    {
        var request = CreateValidRequest();

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validacao_DeveAceitarComplementoAusente()
    {
        var request = CreateValidRequest(complemento: null);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("12345ABC")]
    public void Validacao_DeveRejeitarCepAusenteOuInvalido(string? cep)
    {
        var request = CreateValidRequest(cep: cep!);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(EnderecoRequest.Cep));
    }

    [Theory]
    [InlineData(null, nameof(EnderecoRequest.Logradouro))]
    [InlineData(null, nameof(EnderecoRequest.Numero))]
    [InlineData(null, nameof(EnderecoRequest.Bairro))]
    [InlineData(null, nameof(EnderecoRequest.Cidade))]
    [InlineData(null, nameof(EnderecoRequest.Estado))]
    public void Validacao_DeveRejeitarCamposObrigatoriosAusentes(string? value, string memberName)
    {
        var request = memberName switch
        {
            nameof(EnderecoRequest.Logradouro) => CreateValidRequest(logradouro: value!),
            nameof(EnderecoRequest.Numero) => CreateValidRequest(numero: value!),
            nameof(EnderecoRequest.Bairro) => CreateValidRequest(bairro: value!),
            nameof(EnderecoRequest.Cidade) => CreateValidRequest(cidade: value!),
            nameof(EnderecoRequest.Estado) => CreateValidRequest(estado: value!),
            _ => CreateValidRequest()
        };

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, memberName);
    }

    [Theory]
    [InlineData("S")]
    [InlineData("SPO")]
    public void Validacao_DeveRejeitarEstadoComTamanhoDiferenteDe2(string estado)
    {
        var request = CreateValidRequest(estado: estado);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(EnderecoRequest.Estado));
    }

    private static EnderecoRequest CreateValidRequest(
        string cep = "01001000",
        string logradouro = "Praca da Se",
        string numero = "1",
        string? complemento = "Apto 10",
        string bairro = "Se",
        string cidade = "Sao Paulo",
        string estado = "SP")
    {
        return new EnderecoRequest(cep, logradouro, numero, complemento, bairro, cidade, estado);
    }

    private static List<ValidationResult> Validate(EnderecoRequest request)
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
