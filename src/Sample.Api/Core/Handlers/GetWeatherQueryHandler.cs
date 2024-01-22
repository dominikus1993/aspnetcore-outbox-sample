using Mediator;
using Sample.Api.Core.Model;
using Sample.Api.Core.Query;

namespace Sample.Api.Core.Handlers;

public class GetWeatherQueryHandler : IStreamRequestHandler<GetProductsQuery, Product>
{
    public IAsyncEnumerable<Product> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
