namespace MotoRevApi.Dto.Response;

public record ConcessionariaListResponse
{
    public int Id { get; init; }
    public string Nome { get; init; }
    public bool PossuiEnderecos { get; init; }
    public string? Cidade { get; init; }
    public string? Estado { get; init; }
}
