using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Sample.Api.Tests.Fixtures;

public sealed class MongoFixture : IAsyncLifetime
{
    public readonly MongoDbContainer MongoDbContainer = new MongoDbBuilder().Build();
    public IMongoDatabase Database { get; private set; }
    public IMongoClient Client { get; private set; }
    
    public MongoFixture()
    {
        
    }
    
    public async Task InitializeAsync()
    {
        await MongoDbContainer.StartAsync();
        await MigrateAsync();
    }

    public async Task MigrateAsync()
    {
        await using var context = await DbContextFactory.CreateDbContextAsync();
        await context.Database.MigrateAsync();
    }
    public void Clean()
    {
        using var context = DbContextFactory.CreateDbContext();
        context.CleanAllTables();
    }

    public async Task DisposeAsync()
    {
        await MongoDbContainer.DisposeAsync();
    }
}
