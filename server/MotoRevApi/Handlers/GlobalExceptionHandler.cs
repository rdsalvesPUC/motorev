using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MotoRevApi.Exceptions;
using System.ComponentModel.DataAnnotations;

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
            SecurityTokenException or UnauthorizedAccessException => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Não Autorizado",
                Detail = "O token de acesso ou refresh token fornecido é inválido ou expirou."
            },
            RegistrationException regEx => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Erro de Registro",
                Detail = "Um ou mais erros de validação ocorreram.",
                Extensions = { ["errors"] = regEx.Errors }
            },
            ValidationException validationEx => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Erro de Validação",
                Detail = validationEx.Message
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
            DbUpdateException dbEx when dbEx.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627) => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflito de Dados",
                Detail = "Não foi possível completar a operação pois já existe um registro com os mesmos dados únicos (código ou nome)."
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
