namespace MotoRevApi.Dto.Response;

public record LojaResponse(
    int Id,
    string Nome,
    string Tipo,
    string Cnpj,
    string Telefone,
    string Cep,
    string Logradouro,
    string Numero,
    string Bairro,
    string Cidade,
    string Uf,
    int ConcessionariaId,
    bool Ativo,
    string? Foto = null
);
