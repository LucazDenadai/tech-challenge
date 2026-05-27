namespace OficinaMecanica.Estoque.IntegrationTests.Fixtures;

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<EstoqueWebApplicationFactory>
{
    public const string Name = "Integration";
}
