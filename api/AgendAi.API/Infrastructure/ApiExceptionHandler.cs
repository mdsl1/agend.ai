namespace AgendAi.API.Infrastructure;
using AgendAi.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var (statusCode, titulo, codigo) = exception switch
        {
            ValidacaoException er => (
                StatusCodes.Status400BadRequest,
                "Requisição inválida",
                er.Codigo
            ),

            RecursoNaoEncontradoException er => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                er.Codigo
            ),
            ConflitoException er => (
                StatusCodes.Status409Conflict,
                "Conflito",
                er.Codigo
            ),

            IntegracaoExternaException er => (
                StatusCodes.Status502BadGateway,
                "Falha na integração externa",
                er.Codigo
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno",
                "erro_interno"
            )
        };

        if(exception is AplicacaoException aplicacaoException)
        {
            _logger.LogWarning(
                "Falha tratada pela API. Código: {Codigo}. TraceId: {TraceId}",
                aplicacaoException.Codigo,
                httpContext.TraceIdentifier
            );
        }
        else
        {
            _logger.LogError(
                exception,
                "Erro não tratado. TraceId: {TraceId}",
                httpContext.TraceIdentifier
            );
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = titulo,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "Ocorreu um erro inesperado" 
                : exception.Message,
            Instance = httpContext.Request.Path.ToString()
        };

        problemDetails.Extensions["codigo"] = codigo;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken
        );

        return true;
    }
}
