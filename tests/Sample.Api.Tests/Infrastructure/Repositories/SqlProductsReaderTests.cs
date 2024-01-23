using AutoFixture.Xunit2;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;
using Sample.Api.Infrastructure.Repositories;
using Sample.Api.Tests.Fixtures;

namespace Sample.Api.Tests.Infrastructure.Repositories;

public sealed class SqlProductsReaderTests : IClassFixture<PostgresFixture>, IDisposable
{
    private readonly PostgresFixture _postgresFixture;
    private readonly IProductsReader _productsReader;
    private readonly IProductsWriter _productsWriter;
    private readonly ProductsDbContext _productsDbContext;
    
    public SqlProductsReaderTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _productsDbContext = _postgresFixture.DbContextFactory.CreateDbContext();
        _productsReader = new SqlProductsReader(_productsDbContext);
        _productsWriter = new SqlProductsWriter(_productsDbContext);
    }

    [Theory]
    [AutoData]
    public async Task TestWhenProductExists(Product[] products)
    {
        await _productsWriter.InsertMany(products);

        var productsFromDb = await _productsReader.GetProductsAsync(products.Select(p => p.Id)).ToArrayAsync();
        
        Assert.Equal(products.Length, productsFromDb.Length);
    }

    public void Dispose()
    {
        _productsDbContext.CleanAllTables();
        _productsDbContext.Dispose();
    }
}
