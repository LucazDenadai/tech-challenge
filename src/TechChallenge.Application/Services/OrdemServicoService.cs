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

    public async Task<IEnumerable<OrdemServicoDto>> FiltrarAsync(string? busca, StatusOrdemServico? status)
        => (await _repo.FiltrarAsync(busca, status)).Select(MapDto);

    public async Task<OrdemServicoDto> CriarAsync(CriarOrdemServicoDto dto)
    {
        var cliente = await _clienteRepo.ObterPorDocumentoAsync(dto.DocumentoCliente)
            ?? throw new KeyNotFoundException("Cliente não encontrado para o documento informado.");

        if (!cliente.Ativo)
        {
            throw new InvalidOperationException("O cliente informado está inativo.");
        }

        var placaNormalizada = dto.PlacaVeiculo.Replace("-", "").Replace(" ", "").ToUpperInvariant();
        var veiculo = await _veiculoRepo.ObterPorPlacaAsync(placaNormalizada)
            ?? throw new KeyNotFoundException("Veículo não encontrado para a placa informada.");

        if (veiculo.ClienteId != cliente.Id)
        {
            throw new InvalidOperationException("O veículo não pertence ao cliente informado.");
        }

        var numero = await _repo.GerarNumeroAsync();
        var os = new OrdemServico(numero, cliente.Id, veiculo.Id, dto.Observacoes);
        await _repo.AdicionarAsync(os);
        await _repo.SalvarAsync();
        return MapDto(os);
    }

    public async Task<OrdemServicoDto> AlterarStatusAsync(string numero, AlterarStatusDto dto)
    {
        var os = await _repo.ObterComDetalhesPorNumeroAsync(numero.ToUpperInvariant())
            ?? throw new KeyNotFoundException("Ordem de Serviço não encontrada.");
        os.AlterarStatus(dto.NovoStatus);
        await _repo.AtualizarAsync(os);
        await _repo.SalvarAsync();
        return MapDto(os);
    }

    public async Task<OrdemServicoDto> AdicionarItemAsync(Guid ordemId, AdicionarItemDto dto)
    {
        // 1. Obter e validar ordem de serviço
        var os = await _repo.ObterComDetalhesAsync(ordemId)
            ?? throw new KeyNotFoundException("Ordem de Serviço não encontrada.");

        // Validar status da OS - só permite adicionar itens em EmDiagnostico ou EmExecucao
        if (os.Status != StatusOrdemServico.EmDiagnostico && os.Status != StatusOrdemServico.EmExecucao)
        {
            throw new InvalidOperationException(
                $"Não é possível adicionar itens com a OS no status '{os.Status}'. " +
                $"A OS deve estar em 'EmDiagnostico' ou 'EmExecucao'.");
        }

        // 2. Validar e obter o item (serviço ou peça)
        if (dto.Tipo == TipoItem.Servico)
        {
            // 2a. Para SERVIÇO: apenas validar existência
            var servico = await _servicoRepo.ObterPorIdAsync(dto.ItemId)
                ?? throw new KeyNotFoundException("Serviço não encontrado.");

            if (!servico.Ativo)
            {
                throw new InvalidOperationException(
                    $"O serviço '{servico.Nome}' está inativo e não pode ser adicionado à OS.");
            }

            // 3a. Criar e vincular item de serviço
            os.AdicionarItemServico(new ItemServico(ordemId, dto.ItemId, dto.Quantidade, servico.Preco));
        }
        else
        {
            // 2b. Para PEÇA: validar existência E estoque
            var peca = await _pecaRepo.ObterPorIdAsync(dto.ItemId)
                ?? throw new KeyNotFoundException($"Peça com ID '{dto.ItemId}' não encontrada.");

            if (!peca.Ativo)
            {
                throw new InvalidOperationException(
                    $"A peça '{peca.Nome}' está inativa e não pode ser adicionada à OS.");
            }

            // Validar estoque ANTES de consumir
            if (peca.QuantidadeEstoque < dto.Quantidade)
            {
                throw new InvalidOperationException(
                    $"Estoque insuficiente para a peça '{peca.Nome}'. " +
                    $"Disponível: {peca.QuantidadeEstoque}, Solicitado: {dto.Quantidade}.");
            }

            // 3b. Consumir estoque e vincular item de peça
            peca.ConsumirEstoque(dto.Quantidade);
            os.AdicionarItemPeca(new ItemPeca(ordemId, dto.ItemId, dto.Quantidade, peca.Preco));
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
        if (itemServico != null)
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
            if (peca != null) { await _pecaRepo.AtualizarAsync(peca); }
        }

        await _repo.AtualizarAsync(os);
        await _repo.SalvarAsync();
        return MapDto(os);
    }

    public async Task<AcompanhamentoOsDto?> AcompanharPorNumeroAsync(string numero)
    {
        var os = await _repo.ObterPorNumeroAsync(numero.ToUpperInvariant());
        if (os is null) { return null; }

        return new AcompanhamentoOsDto
        {
            Numero = os.Numero,
            Status = os.Status,
            StatusDescricao = os.Status.ToString(),
            PlacaVeiculo = os.Veiculo?.Placa ?? "",
            MarcaVeiculo = os.Veiculo?.Marca ?? "",
            ModeloVeiculo = os.Veiculo?.Modelo ?? "",
            AnoVeiculo = os.Veiculo?.Ano ?? 0,
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

    public async Task<TempoMedioExecucaoDto> ObterTempoMedioExecucaoAsync()
    {
        var (tempoMedio, total) = await _repo.ObterTempoMedioExecucaoAsync();
        return new TempoMedioExecucaoDto { TempoMedioHoras = tempoMedio, TotalOrdensAnalisadas = total };
    }

    public async Task<TempoIndividualOsDto?> ObterTempoIndividualAsync(string numero)
    {
        var os = await _repo.ObterComDetalhesPorNumeroAsync(numero.ToUpperInvariant());
        if (os is null) { return null; }

        var agora = DateTime.UtcNow;
        var fim = os.DataFechamento ?? agora;
        var totalSpan = fim - os.DataAbertura;

        var historico = os.Historico.OrderBy(h => h.DataAlteracao).ToList();
        var temposPorStatus = new List<TempoPorStatusDto>();

        if (historico.Count == 0)
        {
            temposPorStatus.Add(new TempoPorStatusDto
            {
                Status = os.Status.ToString(),
                TempoHoras = Math.Round(totalSpan.TotalHours, 2),
                TempoFormatado = FormatarTempo(totalSpan)
            });
        }
        else
        {
            var pontos = new List<(DateTime Momento, string Status)>
            {
                (os.DataAbertura, historico[0].StatusAnterior.ToString())
            };
            foreach (var h in historico)
                pontos.Add((h.DataAlteracao, h.StatusNovo.ToString()));

            for (var i = 0; i < pontos.Count; i++)
            {
                var proximoMomento = i + 1 < pontos.Count ? pontos[i + 1].Momento : fim;
                var span = proximoMomento - pontos[i].Momento;
                if (span < TimeSpan.Zero) span = TimeSpan.Zero;
                temposPorStatus.Add(new TempoPorStatusDto
                {
                    Status = pontos[i].Status,
                    TempoHoras = Math.Round(span.TotalHours, 2),
                    TempoFormatado = FormatarTempo(span)
                });
            }
        }

        var obs = os.DataFechamento.HasValue
            ? "OS finalizada."
            : "OS em andamento — tempo calculado até o momento atual.";

        return new TempoIndividualOsDto
        {
            Numero = os.Numero,
            Status = os.Status.ToString(),
            DataAbertura = os.DataAbertura,
            DataFechamento = os.DataFechamento,
            TempoTotalHoras = Math.Round(totalSpan.TotalHours, 2),
            TempoTotalFormatado = FormatarTempo(totalSpan),
            Observacao = obs,
            TemposPorStatus = temposPorStatus
        };
    }

    private static string FormatarTempo(TimeSpan ts)
    {
        var dias = (int)ts.TotalDays;
        var horas = ts.Hours;
        var minutos = ts.Minutes;

        if (dias > 0 && horas > 0) return $"{dias} dia(s) e {horas}h";
        if (dias > 0) return $"{dias} dia(s)";
        if (horas > 0 && minutos > 0) return $"{horas}h {minutos}min";
        if (horas > 0) return $"{horas}h";
        return $"{minutos}min";
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
