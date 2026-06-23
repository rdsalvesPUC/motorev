using System.ComponentModel.DataAnnotations;
using MotoRevApi.Dto.Request;
using Xunit;

namespace MotoRevApi.Tests.Dto.Request;

public class ConcessionariaPerfilRequestTests
{
    [Fact]
    public void Validacao_DeveAceitarRequestValidoComEndereco()
    {
        var request = CreateValidRequest();

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
        Assert.Equal("01001000", request.Cep);
        Assert.Equal("Praca da Se", request.Logradouro);
        Assert.Equal("1", request.Numero);
        Assert.Equal("Se", request.Bairro);
        Assert.Equal("Sao Paulo", request.Cidade);
        Assert.Equal("SP", request.Uf);
    }

    [Fact]
    public void Validacao_DeveAceitarEnderecoOpcionalAusente()
    {
        var request = CreateValidRequest(
            cep: null,
            logradouro: null,
            numero: null,
            bairro: null,
            cidade: null,
            uf: null);

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(nameof(ConcessionariaPerfilRequest.Nome))]
    [InlineData(nameof(ConcessionariaPerfilRequest.Email))]
    [InlineData(nameof(ConcessionariaPerfilRequest.Cnpj))]
    [InlineData(nameof(ConcessionariaPerfilRequest.Telefone))]
    public void Validacao_DeveRejeitarCamposObrigatoriosAusentes(string memberName)
    {
        var request = memberName switch
        {
            nameof(ConcessionariaPerfilRequest.Nome) => CreateValidRequest(nome: null!),
            nameof(ConcessionariaPerfilRequest.Email) => CreateValidRequest(email: null!),
            nameof(ConcessionariaPerfilRequest.Cnpj) => CreateValidRequest(cnpj: null!),
            nameof(ConcessionariaPerfilRequest.Telefone) => CreateValidRequest(telefone: null!),
            _ => CreateValidRequest()
        };

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, memberName);
    }

    [Fact]
    public void Validacao_DeveRejeitarEmailInvalido()
    {
        var request = CreateValidRequest(email: "email-invalido");

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(ConcessionariaPerfilRequest.Email));
    }

    [Theory]
    [InlineData("S")]
    [InlineData("SPO")]
    public void Validacao_DeveRejeitarUfComTamanhoDiferenteDe2(string uf)
    {
        var request = CreateValidRequest(uf: uf);

        var validationResults = Validate(request);

        AssertHasErrorFor(validationResults, nameof(ConcessionariaPerfilRequest.Uf));
    }

    private static ConcessionariaPerfilRequest CreateValidRequest(
        string nome = "Concessionaria Centro",
        string email = "contato@concessionaria.com",
        string cnpj = "12345678000190",
        string telefone = "11999999999",
        string? cep = "01001000",
        string? logradouro = "Praca da Se",
        string? numero = "1",
        string? bairro = "Se",
        string? cidade = "Sao Paulo",
        string? uf = "SP")
    {
        return new ConcessionariaPerfilRequest(
            nome,
            email,
            cnpj,
            telefone,
            cep,
            logradouro,
            numero,
            bairro,
            cidade,
            uf);
    }

    private static List<ValidationResult> Validate(ConcessionariaPerfilRequest request)
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
