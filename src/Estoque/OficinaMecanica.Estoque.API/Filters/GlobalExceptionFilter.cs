using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OficinaMecanica.Estoque.Application.Exceptions;

namespace OficinaMecanica.Estoque.API.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is NotFoundException notFound)
        {
            context.Result = new NotFoundObjectResult(new { mensagem = notFound.Message });
            context.ExceptionHandled = true;
        }
    }
}
