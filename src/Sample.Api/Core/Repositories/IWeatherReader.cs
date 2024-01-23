using Sample.Api.Core.Model;

namespace Sample.Api.Core.Repositories;

public interface IProductsReader
{
    IAsyncEnumerable<Product> GetProductsAsync(IEnumerable<ProductId> productIds, CancellationToken cancellationToken = default);
}
