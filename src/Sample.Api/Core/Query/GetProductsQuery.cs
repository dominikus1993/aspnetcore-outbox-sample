using Mediator;
using Sample.Api.Core.Model;

namespace Sample.Api.Core.Query;

public sealed record GetProductsQuery(IEnumerable<ProductId> ProductIds) : IStreamRequest<Product>
{
}
