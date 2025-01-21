namespace Milvonion.IntegrationTests.TestBase;

[CollectionDefinition(nameof(MilvonionTestCollection))]
public class MilvonionTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}