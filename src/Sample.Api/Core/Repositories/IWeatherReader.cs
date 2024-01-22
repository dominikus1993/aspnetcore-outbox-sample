namespace Sample.Api.Core.Repositories;

public interface IProductReader
{
    IAsyncEnumerable<WeatherForecast> GetProductsAsync(IEnumerable<ProductId> productIds, CancellationToken cancellationToken = default);
}
