namespace MotoRevApi.Exceptions;

/// <summary>
/// Exceção lançada quando uma operação viola uma regra de negócio.
/// Mapeada para HTTP 422 Unprocessable Entity.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
