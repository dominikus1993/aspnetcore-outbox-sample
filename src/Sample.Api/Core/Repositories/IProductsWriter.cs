using Sample.Api.Core.Model;

namespace Sample.Api.Core.Repositories;

public interface IProductsWriter
{
    Task InsertMany(IEnumerable<Product> products, CancellationToken cancellationToken = default);
}
