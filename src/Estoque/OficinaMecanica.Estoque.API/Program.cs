using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Estoque.API.Adapters.In.Messaging;
using OficinaMecanica.Estoque.API.Filters;
using OficinaMecanica.Estoque.Application.UseCases;
using OficinaMecanica.Estoque.Infrastructure;
using OficinaMecanica.Estoque.Infrastructure.Adapters.Out.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure (repositórios, DbContext) ───────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Use Cases ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<GerenciarPecaUseCase>();
builder.Services.AddScoped<ConsultarDisponibilidadeUseCase>();
builder.Services.AddScoped<BaixarEstoqueUseCase>();

// ── Consumers (IHostedService) ─────────────────────────────────────────────────
builder.Services.AddHostedService<BaixaEstoqueConsumer>();

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

// ── Migration automática ───────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

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
