using Agenda.Application.Common.Exceptions;
using Agenda.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Agenda.Api.Common;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException ve)
        {
            var vp = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dados inválidos",
                Instance = httpContext.Request.Path,
            };
            foreach (var group in ve.Errors.GroupBy(e => ToCamelCase(e.PropertyName)))
                vp.Errors[group.Key] = group.Select(e => e.ErrorMessage).ToArray();

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(vp, options: null,
                contentType: "application/problem+json", cancellationToken);
            return true;
        }

        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado", "Contato não encontrado."),
            DuplicateEmailException => (StatusCodes.Status409Conflict, "E-mail duplicado", "Já existe um contato com este e-mail."),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor", "Ocorreu um erro inesperado."),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else if (exception is NotFoundException)
            logger.LogInformation("Not found: {Message}", exception.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, options: null,
            contentType: "application/problem+json", cancellationToken);
        return true;
    }

    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
