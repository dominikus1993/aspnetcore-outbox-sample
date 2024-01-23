using Microsoft.EntityFrameworkCore;
using Sample.Api.Infrastructure.EfCore;
using Testcontainers.PostgreSql;

namespace Sample.Api.Tests.Fixtures;

internal sealed class ProductsDbContextFactory : IDbContextFactory<ProductsDbContext>
{
    private readonly DbContextOptions<ProductsDbContext> _options;

    public ProductsDbContextFactory(DbContextOptionsBuilder<ProductsDbContext> optionsBuilder)
    {
        _options = optionsBuilder.Options;
    }
    public ProductsDbContext CreateDbContext()
    {
        return new ProductsDbContext(_options);
    }
}

public sealed class PostgresFixture : IAsyncLifetime
{
    public readonly PostgreSqlContainer PostgreSqlContainer = new PostgreSqlBuilder().Build();
    public IDbContextFactory<ProductsDbContext> DbContextFactory { get; private set; }
    public PostgresFixture()
    {
        
    }
    
    public async Task InitializeAsync()
    {
        await PostgreSqlContainer.StartAsync();
        var builder = new DbContextOptionsBuilder<ProductsDbContext>()
            .UseNpgsql(this.PostgreSqlContainer.GetConnectionString(),
                optionsBuilder =>
                {
                    optionsBuilder.MigrationsAssembly(typeof(ProductsDbContext).Assembly.FullName);
                    optionsBuilder.EnableRetryOnFailure(5);
                    optionsBuilder.CommandTimeout(500);
                }).UseSnakeCaseNamingConvention();
        DbContextFactory = new ProductsDbContextFactory(builder);
    }

    public void Clean()
    {
        using var context = DbContextFactory.CreateDbContext();
        context.CleanAllTables();
    }

    public async Task DisposeAsync()
    {
        await PostgreSqlContainer.DisposeAsync();
    }
}
