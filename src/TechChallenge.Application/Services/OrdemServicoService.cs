using TechChallenge.Application.DTOs.OrdemServico;
using TechChallenge.Application.Interfaces;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Interfaces;

namespace TechChallenge.Application.Services;

public class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoRepository _repo;
    private readonly IServicoRepository _servicoRepo;
    private readonly IPecaRepository _pecaRepo;
    private readonly IClienteRepository _clienteRepo;
    private readonly IVeiculoRepository _veiculoRepo;

    public OrdemServicoService(
        IOrdemServicoRepository repo,
        IServicoRepository servicoRepo,
        IPecaRepository pecaRepo,
        IClienteRepository clienteRepo,
        IVeiculoRepository veiculoRepo)
    {
        _repo = repo;
        _servicoRepo = servicoRepo;
        _pecaRepo = pecaRepo;
        _clienteRepo = clienteRepo;
        _veiculoRepo = veiculoRepo;
    }

    public async Task<IEnumerable<OrdemServicoDto>> ObterTodosAsync()
    {
        var lista = await _repo.ObterTodosAsync();
        return lista.Select(MapDto);
    }

    public async Task<OrdemServicoDto?> ObterPorIdAsync(Guid id)
    {
        var e = await _repo.ObterComDetalhesAsync(id);
        return e is null ? null : MapDto(e);
    }

    public async Task<IEnumerable<OrdemServicoDto>> ObterPorClienteAsync(Guid clienteId)
        => (await _repo.ObterPorClienteAsync(clienteId)).Select(MapDto);

    public async Task<IEnumerable<OrdemServicoDto>> ObterPorStatusAsync(StatusOrdemServico status)
        => (await _repo.ObterPorStatusAsync(status)).Select(MapDto);

    public async Task<IEnumerable<OrdemServicoDto>> FiltrarAsync(Guid? clienteId, StatusOrdemServico? status)
        => (await _repo.FiltrarAsync(clienteId, status)).Select(MapDto);

    public async Task<OrdemServicoDto> CriarAsync(CriarOrdemServicoDto dto)
    {
        var cliente = await _clienteRepo.ObterPorIdAsync(dto.ClienteId)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");

        var veiculo = await _veiculoRepo.ObterPorIdAsync(dto.VeiculoId)
            ?? throw new KeyNotFoundException("Veículo não encontrado.");

        if (veiculo.ClienteId != cliente.Id)
            throw new InvalidOperationException("O veículo não pertence ao cliente informado.");

        var numero = await _repo.GerarNumeroAsync();
        var os = new OrdemServico(numero, dto.ClienteId, dto.VeiculoId, dto.Observacoes);
        await _repo.AdicionarAsync(os);
        await _repo.SalvarAsync();
        return MapDto(os);
    }

    public async Task<OrdemServicoDto> AlterarStatusAsync(Guid id, AlterarStatusDto dto)
    {
        var os = await _repo.ObterComDetalhesAsync(id)
            ?? throw new KeyNotFoundException("Ordem de Serviço não encontrada.");
        os.AlterarStatus(dto.NovoStatus);
        await _repo.AtualizarAsync(os);
        await _repo.SalvarAsync();
        return MapDto(os);
    }

    public async Task<OrdemServicoDto> AdicionarItemAsync(Guid ordemId, AdicionarItemDto dto)
    {
        var os = await _repo.ObterComDetalhesAsync(ordemId)
            ?? throw new KeyNotFoundException("Ordem de Serviço não encontrada.");

        if (dto.Tipo == TipoItem.Servico)
        {
            var servico = await _servicoRepo.ObterPorIdAsync(dto.ItemId)
                ?? throw new KeyNotFoundException("Serviço não encontrado.");
            os.AdicionarItemServico(new ItemServico(ordemId, dto.ItemId, dto.Quantidade, servico.Preco));
        }
        else
        {
            var peca = await _pecaRepo.ObterPorIdAsync(dto.ItemId)
                ?? throw new KeyNotFoundException("Peça não encontrada.");
            peca.ConsumirEstoque(dto.Quantidade);
            os.AdicionarItemPeca(new ItemPeca(ordemId, dto.ItemId, dto.Quantidade, peca.Preco));
            await _pecaRepo.AtualizarAsync(peca);
        }

        await _repo.AtualizarAsync(os);
        await _repo.SalvarAsync();
        return MapDto(os);
    }

    public async Task<OrdemServicoDto> CancelarItemAsync(Guid ordemId, Guid itemId)
    {
        var os = await _repo.ObterComDetalhesAsync(ordemId)
            ?? throw new KeyNotFoundException("Ordem de Serviço não encontrada.");

        var itemServico = os.ItensServico.FirstOrDefault(i => i.Id == itemId);
        if (itemServico is not null)
        {
            os.RemoverItemServico(itemId);
        }
        else
        {
            var itemPeca = os.ItensPeca.FirstOrDefault(i => i.Id == itemId)
                ?? throw new KeyNotFoundException("Item não encontrado nesta OS.");
            os.RemoverItemPeca(itemId);
            var peca = await _pecaRepo.ObterPorIdAsync(itemPeca.PecaId);
            peca?.AdicionarEstoque(itemPeca.Quantidade);
            if (peca is not null) await _pecaRepo.AtualizarAsync(peca);
        }

        await _repo.AtualizarAsync(os);
        await _repo.SalvarAsync();
        return MapDto(os);
    }

    public async Task<TempoMedioExecucaoDto> ObterTempoMedioExecucaoAsync()
    {
        var (tempoMedio, total) = await _repo.ObterTempoMedioExecucaoAsync();
        return new TempoMedioExecucaoDto { TempoMedioHoras = tempoMedio, TotalOrdensAnalisadas = total };
    }

    private static OrdemServicoDto MapDto(OrdemServico os) => new()
    {
        Id = os.Id,
        Numero = os.Numero,
        ClienteId = os.ClienteId,
        NomeCliente = os.Cliente?.Nome ?? "",
        VeiculoId = os.VeiculoId,
        PlacaVeiculo = os.Veiculo?.Placa ?? "",
        MarcaVeiculo = os.Veiculo?.Marca ?? "",
        ModeloVeiculo = os.Veiculo?.Modelo ?? "",
        AnoVeiculo = os.Veiculo?.Ano ?? 0,
        Status = os.Status,
        StatusDescricao = os.Status.ToString(),
        Observacoes = os.Observacoes,
        DataAbertura = os.DataAbertura,
        DataFechamento = os.DataFechamento,
        ValorTotal = os.ValorTotal,
        ItensServico = os.ItensServico.Select(i => new ItemServicoDto
        {
            Id = i.Id,
            ServicoId = i.ServicoId,
            NomeServico = i.Servico?.Nome ?? "",
            Quantidade = i.Quantidade,
            ValorUnitario = i.ValorUnitario,
            ValorTotal = i.ValorTotal
        }).ToList(),
        ItensPeca = os.ItensPeca.Select(i => new ItemPecaDto
        {
            Id = i.Id,
            PecaId = i.PecaId,
            NomePeca = i.Peca?.Nome ?? "",
            Quantidade = i.Quantidade,
            ValorUnitario = i.ValorUnitario,
            ValorTotal = i.ValorTotal
        }).ToList(),
        Historico = os.Historico
            .OrderBy(h => h.DataAlteracao)
            .Select(h => new HistoricoStatusDto
            {
                StatusAnterior = h.StatusAnterior,
                StatusNovo = h.StatusNovo,
                DataAlteracao = h.DataAlteracao
            }).ToList()
    };
}
