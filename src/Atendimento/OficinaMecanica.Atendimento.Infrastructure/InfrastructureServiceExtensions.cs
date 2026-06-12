using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Email;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Http;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Messaging;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence.Repositories;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Security;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Stubs;
using Polly;
using Polly.Extensions.Http;

namespace OficinaMecanica.Atendimento.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IServicoRepository, ServicoRepository>();

        services.AddScoped<ITokenService, JwtTokenService>();

        var usarEmailReal = configuration.GetValue<bool>("Smtp:Enabled");
        if (usarEmailReal)
            services.AddScoped<IEmailPort, EmailSmtpAdapter>();
        else
            services.AddScoped<IEmailPort, EmailStub>();

        var usarEstoqueReal = configuration.GetValue<bool>("EstoqueHttp:Enabled");

        if (usarEstoqueReal)
        {
            var estoqueUrl = configuration["EstoqueServiceUrl"]
                ?? throw new InvalidOperationException("EstoqueServiceUrl não configurada.");

            services.AddHttpClient<IEstoquePort, EstoqueHttpAdapter>(client =>
                {
                    client.BaseAddress = new Uri(estoqueUrl);
                })
                .AddPolicyHandler(HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(500)))
                .AddPolicyHandler(HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
        }
        else
        {
            services.AddScoped<IEstoquePort, EstoqueHttpStub>();
        }

        var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
        var usarRabbit = configuration.GetValue<bool>("RabbitMq:Enabled");

        if (usarRabbit)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((_, cfg) =>
                {
                    cfg.Host(rabbitHost, "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.Message<OficinaMecanica.Atendimento.Application.Events.OsFinalizadaEvent>(
                        m => m.SetEntityName("os-finalizada"));
                });
            });
            services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
        }
        else
        {
            services.AddScoped<IEventPublisher, EventPublisherStub>();
        }

        return services;
    }
}
