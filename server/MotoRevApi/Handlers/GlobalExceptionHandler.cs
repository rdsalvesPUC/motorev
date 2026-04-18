using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Exceptions;

namespace MotoRevApi.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true;
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            RegistrationException regEx => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Erro de Registro",
                Detail = "Um ou mais erros de validação ocorreram.",
                Extensions = { ["errors"] = regEx.Errors }
            },
            NotFoundException notFoundEx => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Recurso Não Encontrado",
                Detail = notFoundEx.Message
            },
            DuplicateDataException dupEx => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Dados Duplicados",
                Detail = dupEx.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro Interno no Servidor",
                Detail = exception.Message
            }
        };
    }
}
