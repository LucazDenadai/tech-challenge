using System.Net.Http.Json;
using System.Text.Json;
using OficinaMecanica.Atendimento.Application.Ports.Out;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Http;

public class EstoqueHttpAdapter(HttpClient httpClient) : IEstoquePort
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<bool> VerificarDisponibilidadeAsync(IEnumerable<ItemPecaRequest> itens, CancellationToken ct = default)
    {
        var payload = itens.Select(i => new { pecaId = i.PecaId, quantidade = i.Quantidade });
        var response = await httpClient.PostAsJsonAsync("/estoque/disponibilidade", new { itens = payload }, ct);

        if (!response.IsSuccessStatusCode)
            return false;

        var resultado = await response.Content.ReadFromJsonAsync<DisponibilidadeResponse>(JsonOptions, ct);
        return resultado?.Disponivel ?? false;
    }

    private record DisponibilidadeResponse(bool Disponivel);
}
