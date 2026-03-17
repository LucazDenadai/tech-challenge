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
            new Usuario("Admin Sistema", "admin@oficina.com", BCrypt.Net.BCrypt.HashPassword("Admin@123"), PerfilUsuario.Admin),
            new Usuario("João Mecânico", "mecanico@oficina.com", BCrypt.Net.BCrypt.HashPassword("Mec@123"), PerfilUsuario.Mecanico),
            new Usuario("Maria Atendente", "atendente@oficina.com", BCrypt.Net.BCrypt.HashPassword("Ate@123"), PerfilUsuario.Atendente)
        };
        await context.Usuarios.AddRangeAsync(usuarios);

        // Clientes
        var cliente1 = new Cliente("Carlos Silva", "12345678901", "carlos@email.com", "11987654321", "Rua das Flores, 100");
        var cliente2 = new Cliente("Ana Souza", "98765432100", "ana@email.com", "11912345678", "Av. Paulista, 500");
        var cliente3 = new Cliente("Roberto Lima", "45678901234", "roberto@email.com", "11955556666", "Rua XV, 200");
        await context.Clientes.AddRangeAsync(cliente1, cliente2, cliente3);
        await context.SaveChangesAsync();

        // Veiculos
        var veiculos = new[]
        {
            new Veiculo(cliente1.Id, "ABC1234", "Toyota", "Corolla", 2020, "Prata"),
            new Veiculo(cliente1.Id, "DEF5678", "Honda", "Civic", 2019, "Preto"),
            new Veiculo(cliente2.Id, "GHI9012", "Volkswagen", "Golf", 2022, "Branco"),
            new Veiculo(cliente3.Id, "JKL3456", "Chevrolet", "Onix", 2021, "Vermelho")
        };
        await context.Veiculos.AddRangeAsync(veiculos);

        // Servicos
        var servicos = new[]
        {
            new Servico("Troca de Óleo", "Troca completa do óleo do motor", 80.00m, 30),
            new Servico("Alinhamento", "Alinhamento das rodas dianteiras e traseiras", 120.00m, 60),
            new Servico("Balanceamento", "Balanceamento de todos os pneus", 100.00m, 45),
            new Servico("Revisão Completa", "Revisão geral do veículo com 50 itens", 350.00m, 180),
            new Servico("Troca de Pastilhas de Freio", "Substituição das pastilhas dianteiras", 180.00m, 90)
        };
        await context.Servicos.AddRangeAsync(servicos);

        // Pecas
        var pecas = new[]
        {
            new Peca("Filtro de Óleo", "Filtro de óleo universal", 35.00m, 50),
            new Peca("Pastilha de Freio Dianteira", "Par de pastilhas originais", 120.00m, 30),
            new Peca("Correia Dentada", "Correia dentada 120 dentes", 85.00m, 20),
            new Peca("Vela de Ignição", "Kit com 4 velas NGK", 90.00m, 40),
            new Peca("Amortecedor Dianteiro", "Amortecedor a gás", 280.00m, 10)
        };
        await context.Pecas.AddRangeAsync(pecas);

        await context.SaveChangesAsync();
    }
}
