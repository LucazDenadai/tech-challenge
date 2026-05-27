namespace OficinaMecanica.Estoque.API.Adapters.In.Messaging;

public class BaixaEstoqueConsumerStub(ILogger<BaixaEstoqueConsumerStub> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BaixaEstoqueConsumer iniciado (stub — RabbitMq:Enabled=false)");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
