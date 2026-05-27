using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Messaging;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence.Repositories;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Security;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Stubs;

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
        services.AddScoped<IPecaRepository, PecaRepository>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IEstoquePort, EstoqueHttpStub>();
        services.AddScoped<IEmailPort, EmailStub>();

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
