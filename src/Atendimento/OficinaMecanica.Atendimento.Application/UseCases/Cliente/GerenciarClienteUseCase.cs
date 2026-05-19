using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using DomainCliente = OficinaMecanica.Atendimento.Domain.Entities.Cliente;

namespace OficinaMecanica.Atendimento.Application.UseCases.Cliente;

public class GerenciarClienteUseCase(IClienteRepository repository)
{
    public async Task<Guid> CriarAsync(CriarClienteRequest request, CancellationToken ct = default)
    {
        var documentoSanitizado = DomainCliente.Sanitizar(request.Documento);

        var existe = await repository.DocumentoExisteAsync(documentoSanitizado, ct: ct);
        if (existe)
        {
            var existente = await repository.ObterPorDocumentoAsync(documentoSanitizado, ct);
            if (existente is not null && !existente.Ativo)
            {
                existente.Ativar(request.Nome, request.Email, request.Telefone, request.Endereco);
                await repository.AtualizarAsync(existente, ct);
                await repository.SalvarAsync(ct);
                return existente.Id;
            }
            throw new InvalidOperationException($"Já existe um cliente ativo com o documento informado.");
        }

        var cliente = new DomainCliente(request.Nome, request.Documento, request.Email, request.Telefone, request.Endereco);
        await repository.AdicionarAsync(cliente, ct);
        await repository.SalvarAsync(ct);
        return cliente.Id;
    }

    public async Task AtualizarAsync(AtualizarClienteRequest request, CancellationToken ct = default)
    {
        var cliente = await repository.ObterPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Cliente", request.Id);

        cliente.Atualizar(request.Nome, request.Email, request.Telefone, request.Endereco);
        await repository.AtualizarAsync(cliente, ct);
        await repository.SalvarAsync(ct);
    }

    public async Task DesativarAsync(Guid id, CancellationToken ct = default)
    {
        var cliente = await repository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException("Cliente", id);

        cliente.Desativar();
        await repository.AtualizarAsync(cliente, ct);
        await repository.SalvarAsync(ct);
    }
}
