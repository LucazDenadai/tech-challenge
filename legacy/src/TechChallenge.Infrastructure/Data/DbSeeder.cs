using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Usuarios.AnyAsync()) return;

        // Usuarios
        var usuarios = new[]
        {
            new Usuario("Admin Sistema", "admin@oficina.com", BCrypt.Net.BCrypt.HashPassword("Admin@1234"), PerfilUsuario.Admin),
            new Usuario("João Mecânico", "mecanico@oficina.com", BCrypt.Net.BCrypt.HashPassword("Mecan@123"), PerfilUsuario.Mecanico),
            new Usuario("Maria Atendente", "atendente@oficina.com", BCrypt.Net.BCrypt.HashPassword("Atend@123"), PerfilUsuario.Atendente),
            new Usuario("Demo Atendente", "demo@oficina.com", BCrypt.Net.BCrypt.HashPassword("Demo@1234"), PerfilUsuario.Atendente)
        };
        await context.Usuarios.AddRangeAsync(usuarios);

        // Clientes
        var carlos = new Cliente("Carlos Silva", "52998224725", "carlos@email.com", "11987654321", "Rua das Flores, 100");
        var ana = new Cliente("Ana Souza", "98765432100", "ana@email.com", "11912345678", "Av. Paulista, 500");
        var roberto = new Cliente("Roberto Lima", "11144477735", "roberto@email.com", "11955556666", "Rua XV, 200");
        await context.Clientes.AddRangeAsync(carlos, ana, roberto);
        await context.SaveChangesAsync();

        var corollaSilva = new Veiculo(carlos.Id, "ABC1234", "Toyota", "Corolla", 2020, "Prata");
        var civicSilva = new Veiculo(carlos.Id, "DEF5678", "Honda", "Civic", 2019, "Preto");
        var golfAna = new Veiculo(ana.Id, "GHI9012", "Volkswagen", "Golf", 2022, "Branco");
        var onixRoberto = new Veiculo(roberto.Id, "JKL3456", "Chevrolet", "Onix", 2021, "Vermelho");
        await context.Veiculos.AddRangeAsync(corollaSilva, civicSilva, golfAna, onixRoberto);

        // Servicos
        var trocaOleo = new Servico("Troca de Óleo", "Troca completa do óleo do motor", 80.00m, 30);
        var alinhamento = new Servico("Alinhamento", "Alinhamento das rodas dianteiras e traseiras", 120.00m, 60);
        var balanceamento = new Servico("Balanceamento", "Balanceamento de todos os pneus", 100.00m, 45);
        var revisao = new Servico("Revisão Completa", "Revisão geral do veículo com 50 itens", 350.00m, 180);
        var trocaPastilhas = new Servico("Troca de Pastilhas de Freio", "Substituição das pastilhas dianteiras", 180.00m, 90);
        await context.Servicos.AddRangeAsync(trocaOleo, alinhamento, balanceamento, revisao, trocaPastilhas);

        // Pecas
        var filtroOleo = new Peca("Filtro de Óleo", "Filtro de óleo universal", 35.00m, 50);
        var pastilha = new Peca("Pastilha de Freio Dianteira", "Par de pastilhas originais", 120.00m, 30);
        var correia = new Peca("Correia Dentada", "Correia dentada 120 dentes", 85.00m, 20);
        var vela = new Peca("Vela de Ignição", "Kit com 4 velas NGK", 90.00m, 40);
        var amortecedor = new Peca("Amortecedor Dianteiro", "Amortecedor a gás", 280.00m, 10);
        await context.Pecas.AddRangeAsync(filtroOleo, pastilha, correia, vela, amortecedor);

        await context.SaveChangesAsync();

        // Ordens de Serviço
        // Itens são sempre adicionados em EmDiagnostico, depois o status avança ao destino final.

        // OS-001 — Carlos / Corolla — Troca de Óleo + Filtro → Entregue
        var os001 = new OrdemServico("OS-2026-0001", carlos.Id, corollaSilva.Id, "Troca de óleo de rotina.");
        os001.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os001.AdicionarItemServico(new ItemServico(os001.Id, trocaOleo.Id, 1, trocaOleo.Preco));
        os001.AdicionarItemPeca(new ItemPeca(os001.Id, filtroOleo.Id, 1, filtroOleo.Preco));
        os001.AlterarStatus(StatusOrdemServico.AguardandoAprovacao);
        os001.AlterarStatus(StatusOrdemServico.EmExecucao);
        os001.AlterarStatus(StatusOrdemServico.Finalizada);
        os001.AlterarStatus(StatusOrdemServico.Entregue);

        // OS-002 — Carlos / Corolla — Alinhamento + Balanceamento → Finalizada
        var os002 = new OrdemServico("OS-2026-0002", carlos.Id, corollaSilva.Id, "Cliente relatou vibração no volante.");
        os002.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os002.AdicionarItemServico(new ItemServico(os002.Id, alinhamento.Id, 1, alinhamento.Preco));
        os002.AdicionarItemServico(new ItemServico(os002.Id, balanceamento.Id, 1, balanceamento.Preco));
        os002.AlterarStatus(StatusOrdemServico.AguardandoAprovacao);
        os002.AlterarStatus(StatusOrdemServico.EmExecucao);
        os002.AlterarStatus(StatusOrdemServico.Finalizada);

        // OS-003 — Carlos / Civic — Revisão Completa + Correia + Velas → EmExecucao
        var os003 = new OrdemServico("OS-2026-0003", carlos.Id, civicSilva.Id, "Revisão dos 60.000 km.");
        os003.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os003.AdicionarItemServico(new ItemServico(os003.Id, revisao.Id, 1, revisao.Preco));
        os003.AdicionarItemPeca(new ItemPeca(os003.Id, correia.Id, 1, correia.Preco));
        os003.AdicionarItemPeca(new ItemPeca(os003.Id, vela.Id, 1, vela.Preco));
        os003.AlterarStatus(StatusOrdemServico.AguardandoAprovacao);
        os003.AlterarStatus(StatusOrdemServico.EmExecucao);

        // OS-004 — Ana / Golf — Troca de Pastilhas + Pastilha → AguardandoAprovacao
        var os004 = new OrdemServico("OS-2026-0004", ana.Id, golfAna.Id, "Barulho ao frear. Aguardando aprovação do orçamento.");
        os004.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os004.AdicionarItemServico(new ItemServico(os004.Id, trocaPastilhas.Id, 1, trocaPastilhas.Preco));
        os004.AdicionarItemPeca(new ItemPeca(os004.Id, pastilha.Id, 1, pastilha.Preco));
        os004.AlterarStatus(StatusOrdemServico.AguardandoAprovacao);

        // OS-005 — Ana / Golf — Alinhamento → EmDiagnostico (mecânico avaliando)
        var os005 = new OrdemServico("OS-2026-0005", ana.Id, golfAna.Id, "Pneus desgastando de forma irregular.");
        os005.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os005.AdicionarItemServico(new ItemServico(os005.Id, alinhamento.Id, 1, alinhamento.Preco));

        // OS-006 — Roberto / Onix — sem itens ainda → Recebida (recém aberta)
        var os006 = new OrdemServico("OS-2026-0006", roberto.Id, onixRoberto.Id, "Veículo com suspensão comprometida.");

        // OS-007 — Roberto / Onix — Troca de Óleo + Filtro + Velas → EmDiagnostico
        var os007 = new OrdemServico("OS-2026-0007", roberto.Id, onixRoberto.Id, "Manutenção preventiva.");
        os007.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os007.AdicionarItemServico(new ItemServico(os007.Id, trocaOleo.Id, 1, trocaOleo.Preco));
        os007.AdicionarItemPeca(new ItemPeca(os007.Id, filtroOleo.Id, 1, filtroOleo.Preco));
        os007.AdicionarItemPeca(new ItemPeca(os007.Id, vela.Id, 1, vela.Preco));

        // OS-008 — Carlos / Civic — Balanceamento → Entregue
        var os008 = new OrdemServico("OS-2026-0008", carlos.Id, civicSilva.Id, "Balanceamento após troca de pneus.");
        os008.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os008.AdicionarItemServico(new ItemServico(os008.Id, balanceamento.Id, 1, balanceamento.Preco));
        os008.AlterarStatus(StatusOrdemServico.AguardandoAprovacao);
        os008.AlterarStatus(StatusOrdemServico.EmExecucao);
        os008.AlterarStatus(StatusOrdemServico.Finalizada);
        os008.AlterarStatus(StatusOrdemServico.Entregue);

        await context.OrdensServico.AddRangeAsync(os001, os002, os003, os004, os005, os006, os007, os008);
        await context.SaveChangesAsync();

        // Ajusta datas para tempo médio realista (1-3 dias) nas OS finalizadas/entregues
        await context.Database.ExecuteSqlRawAsync(@"
            UPDATE ""OrdensServico"" SET ""DataAbertura"" = NOW() - INTERVAL '3 days', ""DataFechamento"" = NOW() - INTERVAL '1 day'
            WHERE ""Numero"" IN ('OS-2026-0001', 'OS-2026-0008');
            UPDATE ""OrdensServico"" SET ""DataAbertura"" = NOW() - INTERVAL '5 days', ""DataFechamento"" = NOW() - INTERVAL '3 days'
            WHERE ""Numero"" = 'OS-2026-0002';

            WITH ranked AS (
                SELECT h.""Id"",
                       ROW_NUMBER() OVER (PARTITION BY h.""OrdemServicoId"" ORDER BY h.""DataAlteracao"") AS rn,
                       COUNT(*) OVER (PARTITION BY h.""OrdemServicoId"") AS cnt,
                       os.""DataAbertura"",
                       os.""DataFechamento""
                FROM ""HistoricoStatusOS"" h
                JOIN ""OrdensServico"" os ON os.""Id"" = h.""OrdemServicoId""
                WHERE os.""Numero"" IN ('OS-2026-0001', 'OS-2026-0002', 'OS-2026-0008')
            )
            UPDATE ""HistoricoStatusOS"" h
            SET ""DataAlteracao"" = r.""DataAbertura"" + (r.""DataFechamento"" - r.""DataAbertura"") * r.rn::float / (r.cnt + 1)
            FROM ranked r
            WHERE h.""Id"" = r.""Id"";
        ");
    }
}
