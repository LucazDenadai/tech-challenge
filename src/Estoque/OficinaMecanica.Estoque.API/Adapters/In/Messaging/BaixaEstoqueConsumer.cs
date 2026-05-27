namespace OficinaMecanica.Estoque.API.Adapters.In.Messaging;

public class BaixaEstoqueConsumer(ILogger<BaixaEstoqueConsumer> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BaixaEstoqueConsumer iniciado (stub — aguardando CARD-09)");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
