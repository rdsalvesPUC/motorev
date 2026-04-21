using Microsoft.AspNetCore.Identity;

namespace MotoRevApi.Exceptions;

public class RegistrationException : Exception
{
    public IEnumerable<IdentityError> Errors { get; }

    public RegistrationException(IEnumerable<IdentityError> errors)
    {
        Errors = errors;
    }
}
