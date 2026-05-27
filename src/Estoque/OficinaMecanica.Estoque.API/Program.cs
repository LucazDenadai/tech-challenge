using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Estoque.API.Adapters.In.Messaging;
using OficinaMecanica.Estoque.API.Filters;
using OficinaMecanica.Estoque.Application.Events;
using OficinaMecanica.Estoque.Application.UseCases;
using OficinaMecanica.Estoque.Infrastructure;
using OficinaMecanica.Estoque.Infrastructure.Adapters.In.Messaging;
using OficinaMecanica.Estoque.Infrastructure.Adapters.Out.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure (repositórios, DbContext) ───────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Use Cases ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<GerenciarPecaUseCase>();
builder.Services.AddScoped<ConsultarDisponibilidadeUseCase>();
builder.Services.AddScoped<BaixarEstoqueUseCase>();

// ── MassTransit + RabbitMQ ─────────────────────────────────────────────────────
var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var usarRabbit = builder.Configuration.GetValue<bool>("RabbitMq:Enabled");

if (usarRabbit)
{
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<BaixaEstoqueConsumer>();
        x.AddConsumer<BaixaEstoqueConsumerFaultConsumer>();

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(rabbitHost, "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });

            cfg.ReceiveEndpoint("estoque.baixa", e =>
            {
                e.UseMessageRetry(r => r.Intervals(1000, 5000, 10000));
                e.ConfigureConsumer<BaixaEstoqueConsumer>(ctx);
            });

            cfg.ReceiveEndpoint("estoque.falhas", e =>
            {
                e.ConfigureConsumer<BaixaEstoqueConsumerFaultConsumer>(ctx);
            });

            // Registra o tipo do evento para que o MassTransit crie o exchange correto
            cfg.Message<OsFinalizadaEvent>(x => x.SetEntityName("os-finalizada"));
        });
    });
}
else
{
    // Stub para rodar sem RabbitMQ (desenvolvimento local, testes)
    builder.Services.AddHostedService<BaixaEstoqueConsumerStub>();
}

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
