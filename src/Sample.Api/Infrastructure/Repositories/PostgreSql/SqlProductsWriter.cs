using Microsoft.EntityFrameworkCore;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;

namespace Sample.Api.Infrastructure.Repositories.PostgreSql;

public sealed class SqlProductsWriter : IProductsWriter
{
    private readonly ProductsDbContext _productsDbContext;

    public SqlProductsWriter(ProductsDbContext productsDbContext)
    {
        _productsDbContext = productsDbContext;
    }

    public async Task InsertMany(IEnumerable<Product> products, CancellationToken cancellationToken = default)
    {
        await _productsDbContext.UpsertRange(products).On(p => p.Id).RunAsync(cancellationToken);
    }
}
