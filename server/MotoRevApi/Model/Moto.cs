namespace MotoRevApi.Model;

public class Moto
{
    public int Id { get; set; }
    public required string Modelo { get; set; }
    public required string Cor { get; set; }
    public required string Ano { get; set; }
}