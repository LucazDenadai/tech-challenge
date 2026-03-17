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

    public OrdemServicoService(IOrdemServicoRepository repo, IServicoRepository servicoRepo, IPecaRepository pecaRepo)
    {
        _repo = repo;
        _servicoRepo = servicoRepo;
        _pecaRepo = pecaRepo;
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

    public async Task<OrdemServicoDto> CriarAsync(CriarOrdemServicoDto dto)
    {
        var numero = await _repo.GerarNumeroAsync();
        var os = new OrdemServico(numero, dto.ClienteId, dto.VeiculoId, dto.Observacoes);
        await _repo.AdicionarAsync(os);
        await _repo.SalvarAsync();
        return MapDto(os);
    }

    public async Task<OrdemServicoDto> AvancarStatusAsync(Guid id)
    {
        var os = await _repo.ObterComDetalhesAsync(id)
            ?? throw new KeyNotFoundException("Ordem de Serviço não encontrada.");
        os.AvancarStatus();
        await _repo.AtualizarAsync(os);
        await _repo.SalvarAsync();
        return MapDto(os);
    }

    public async Task<OrdemServicoDto> AdicionarItemServicoAsync(Guid ordemId, AdicionarItemServicoDto dto)
    {
        var os = await _repo.ObterComDetalhesAsync(ordemId)
            ?? throw new KeyNotFoundException("Ordem de Serviço não encontrada.");
        var servico = await _servicoRepo.ObterPorIdAsync(dto.ServicoId)
            ?? throw new KeyNotFoundException("Serviço não encontrado.");

        var item = new ItemServico(ordemId, dto.ServicoId, dto.Quantidade, servico.Preco);
        os.AdicionarItemServico(item);
        await _repo.AtualizarAsync(os);
        await _repo.SalvarAsync();
        return MapDto(os);
    }

    public async Task<OrdemServicoDto> AdicionarItemPecaAsync(Guid ordemId, AdicionarItemPecaDto dto)
    {
        var os = await _repo.ObterComDetalhesAsync(ordemId)
            ?? throw new KeyNotFoundException("Ordem de Serviço não encontrada.");
        var peca = await _pecaRepo.ObterPorIdAsync(dto.PecaId)
            ?? throw new KeyNotFoundException("Peça não encontrada.");

        peca.ConsumirEstoque(dto.Quantidade);
        var item = new ItemPeca(ordemId, dto.PecaId, dto.Quantidade, peca.Preco);
        os.AdicionarItemPeca(item);
        await _repo.AtualizarAsync(os);
        await _pecaRepo.AtualizarAsync(peca);
        await _repo.SalvarAsync();
        return MapDto(os);
    }

    private static OrdemServicoDto MapDto(OrdemServico os) => new()
    {
        Id = os.Id,
        Numero = os.Numero,
        ClienteId = os.ClienteId,
        NomeCliente = os.Cliente?.Nome ?? "",
        VeiculoId = os.VeiculoId,
        PlacaVeiculo = os.Veiculo?.Placa ?? "",
        Status = os.Status,
        StatusDescricao = os.Status.ToString(),
        Observacoes = os.Observacoes,
        DataAbertura = os.DataAbertura,
        DataFechamento = os.DataFechamento,
        ValorTotal = os.ValorTotal,
        ItensServico = os.ItensServico.Select(i => new ItemServicoDto
        {
            Id = i.Id, ServicoId = i.ServicoId, NomeServico = i.Servico?.Nome ?? "",
            Quantidade = i.Quantidade, ValorUnitario = i.ValorUnitario, ValorTotal = i.ValorTotal
        }).ToList(),
        ItensPeca = os.ItensPeca.Select(i => new ItemPecaDto
        {
            Id = i.Id, PecaId = i.PecaId, NomePeca = i.Peca?.Nome ?? "",
            Quantidade = i.Quantidade, ValorUnitario = i.ValorUnitario, ValorTotal = i.ValorTotal
        }).ToList()
    };
}
