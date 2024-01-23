using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;
using Sample.Api.Infrastructure.Repositories;
using Sample.Api.Tests.Fixtures;

namespace Sample.Api.Tests.Infrastructure.Repositories;

public sealed class SqlProductsReaderTests : IClassFixture<PostgresFixture>, IDisposable, IAsyncDisposable
{
    private readonly PostgresFixture _postgresFixture;
    private readonly IProductsReader _productsReader;
    private readonly IProductsWriter _productsWriter;
    private readonly ProductsDbContext _productsDbContext;
    
    SqlProductsReaderTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _productsDbContext = _postgresFixture.DbContextFactory.CreateDbContext();
        _productsReader = new SqlProductsReader(_productsDbContext);
        _productsWriter = new SqlProductsWriter(_productsDbContext);
    }

    public void Dispose()
    {
        _productsDbContext.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _productsDbContext.DisposeAsync();
    }
}
