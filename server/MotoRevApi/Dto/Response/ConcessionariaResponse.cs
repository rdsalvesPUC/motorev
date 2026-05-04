namespace MotoRevApi.Dto.Response;

public record ConcessionariaResponse
{
    public int Id { get; init; }
    public string Nome { get; init; }
    public string Email { get; init; } // Adicionando o email no retorno
    private string _cnpj;
    
    public string Cnpj 
    { 
        get => FormatCnpj(_cnpj); 
        init => _cnpj = value; 
    }

    public List<EnderecoResponse> Enderecos { get; init; }

    private static string FormatCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj) || cnpj.Length != 14)
            return cnpj;

        return Convert.ToUInt64(cnpj).ToString(@"00\.000\.000\/0000\-00");
    }
}
