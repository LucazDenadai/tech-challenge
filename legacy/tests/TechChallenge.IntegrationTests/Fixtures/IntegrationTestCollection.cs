using Xunit;

namespace TechChallenge.IntegrationTests.Fixtures;

/// <summary>
/// Define uma coleção de testes de integração que compartilha UMA única instância da factory
/// (e portanto um único container PostgreSQL) entre todas as classes de teste.
/// Isso evita race conditions nas variáveis de ambiente e reduz o tempo de boot.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
