namespace MotoRevApi.Dto.Response;

public record ConcessionariaListResponse
{
    public int Id { get; init; }
    public string Nome { get; init; }
    public List<EnderecoResponse> Enderecos { get; init; } = new();
}
