using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Sample.Api.Core.Model;
using Sample.Api.Core.Repositories;
using Sample.Api.Infrastructure.EfCore;

namespace Sample.Api.Infrastructure.Repositories.PostgreSql;

public sealed class SqlProductsReader : IProductsReader
{
    private readonly ProductsDbContext _productsDbContext;

    public SqlProductsReader(ProductsDbContext productsDbContext)
    {
        _productsDbContext = productsDbContext;
    }

    public async IAsyncEnumerable<Product> GetProductsAsync(IEnumerable<ProductId> productIds,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var result = _productsDbContext.Products.Where(p => productIds.Contains(p.Id)).AsAsyncEnumerable();
        await foreach(var item in result.WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }
}
