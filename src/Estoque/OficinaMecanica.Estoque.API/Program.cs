using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Estoque.API.Filters;
using OficinaMecanica.Estoque.Application.UseCases;
using OficinaMecanica.Estoque.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure (repositórios, DbContext) ───────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Use Cases ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<GerenciarPecaUseCase>();
builder.Services.AddScoped<ConsultarDisponibilidadeUseCase>();
builder.Services.AddScoped<BaixarEstoqueUseCase>();

// ── Controllers + Filters ──────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var resultado = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Dados inválidos",
            Detail = "Um ou mais campos não passaram na validação."
        };
        return new BadRequestObjectResult(resultado);
    };
});

// ── Swagger ────────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Estoque API v1"));
}

app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();

public partial class Program { protected Program() { } }
