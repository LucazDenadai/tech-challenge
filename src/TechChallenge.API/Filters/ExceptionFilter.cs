using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TechChallenge.API.Filters;

/// <summary>
/// Filtro global de exceções que mapeia exceções de negócio para respostas HTTP apropriadas.
/// </summary>
public class ExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;

        _logger.LogError(exception, "Exceção não tratada: {Message}", exception.Message);

        context.Result = exception switch
        {
            KeyNotFoundException => new NotFoundObjectResult(new { message = exception.Message }),
            InvalidOperationException => new BadRequestObjectResult(new { message = exception.Message }),
            ArgumentException => new BadRequestObjectResult(new { message = exception.Message }),
            _ => new ObjectResult(new { message = "Erro interno do servidor." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };

        context.ExceptionHandled = true;
    }
}
