using System.Text.Json.Serialization;

namespace MotoRevApi.Enums;

public enum CategoriaPeca
{
    Filtros,
    Motor,
    Freios,
    [JsonStringEnumMemberName("Transmissão")]
    Transmissao,
    [JsonStringEnumMemberName("Elétrica")]
    Eletrica
}
