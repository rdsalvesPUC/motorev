namespace MotoRevApi.Dto.Response;

public record ConcessionariaResponse(
    int Id,
    string Nome,
    string Cnpj,
    string Telefone,
    string Tipo,
    string Cep,
    string Logradouro,
    string Numero,
    string Bairro,
    string Cidade,
    string Uf,
    IEnumerable<LojaResponse> Lojas
);
