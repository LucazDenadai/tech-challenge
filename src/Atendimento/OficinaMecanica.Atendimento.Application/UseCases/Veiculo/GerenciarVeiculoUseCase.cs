using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Domain.Validators;
using DomainVeiculo = OficinaMecanica.Atendimento.Domain.Entities.Veiculo;

namespace OficinaMecanica.Atendimento.Application.UseCases.Veiculo;

public class GerenciarVeiculoUseCase(IVeiculoRepository veiculoRepository, IClienteRepository clienteRepository)
{
    public async Task<Guid> CriarAsync(CriarVeiculoRequest request, CancellationToken ct = default)
    {
        var clienteExiste = await clienteRepository.ObterPorIdAsync(request.ClienteId, ct);
        if (clienteExiste is null)
            throw new NotFoundException("Cliente", request.ClienteId);

        var placaNormalizada = PlacaValidator.Normalizar(request.Placa);
        var placaDuplicada = await veiculoRepository.ObterPorPlacaAsync(placaNormalizada, ct);
        if (placaDuplicada is not null)
            throw new InvalidOperationException($"Já existe um veículo com a placa '{placaNormalizada}'.");

        var veiculo = new DomainVeiculo(request.ClienteId, request.Placa, request.Marca, request.Modelo, request.Ano, request.Cor);
        await veiculoRepository.AdicionarAsync(veiculo, ct);
        await veiculoRepository.SalvarAsync(ct);
        return veiculo.Id;
    }

    public async Task AtualizarAsync(AtualizarVeiculoRequest request, CancellationToken ct = default)
    {
        var veiculo = await veiculoRepository.ObterPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Veiculo", request.Id);

        veiculo.Atualizar(request.Marca, request.Modelo, request.Ano, request.Cor);
        await veiculoRepository.AtualizarAsync(veiculo, ct);
        await veiculoRepository.SalvarAsync(ct);
    }
}
