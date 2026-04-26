using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using TechChallenge.API.Filters;
using Xunit;

namespace TechChallenge.IntegrationTests.Controllers;

public class ExceptionFilterTests
{
    private static ExceptionContext CriarContexto(Exception ex)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        return new ExceptionContext(actionContext, []) { Exception = ex };
    }

    [Fact]
    public void OnException_KeyNotFoundException_DeveRetornar404EMensagem()
    {
        var filter = new ExceptionFilter(NullLogger<ExceptionFilter>.Instance);
        var context = CriarContexto(new KeyNotFoundException("recurso não encontrado"));

        filter.OnException(context);

        var result = context.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        result.StatusCode.Should().Be(404);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public void OnException_InvalidOperationException_DeveRetornar400()
    {
        var filter = new ExceptionFilter(NullLogger<ExceptionFilter>.Instance);
        var context = CriarContexto(new InvalidOperationException("operação inválida"));

        filter.OnException(context);

        context.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.StatusCode.Should().Be(400);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public void OnException_ArgumentException_DeveRetornar400()
    {
        var filter = new ExceptionFilter(NullLogger<ExceptionFilter>.Instance);
        var context = CriarContexto(new ArgumentException("argumento inválido"));

        filter.OnException(context);

        context.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.StatusCode.Should().Be(400);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public void OnException_ExcecaoGenerica_DeveRetornar500()
    {
        var filter = new ExceptionFilter(NullLogger<ExceptionFilter>.Instance);
        var context = CriarContexto(new Exception("erro inesperado"));

        filter.OnException(context);

        var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(500);
        context.ExceptionHandled.Should().BeTrue();
    }
}
