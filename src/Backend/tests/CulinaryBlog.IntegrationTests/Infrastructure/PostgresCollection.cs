namespace CulinaryBlog.IntegrationTests.Infrastructure;

/// <summary>Các lớp test dùng chung một container PostgreSQL: gắn <c>[Collection(PostgresCollection.Name)]</c>.</summary>
[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresApiFactory>
{
    public const string Name = "Postgres";
}
