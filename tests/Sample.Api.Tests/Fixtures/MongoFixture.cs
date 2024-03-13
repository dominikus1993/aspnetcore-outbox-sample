using Microsoft.EntityFrameworkCore.Internal;
using MongoDB.Driver;
using Sample.Api.Infrastructure.Repositories.MongoDb;
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
        var (mongoClient, mongoDatabase) = MongoDbExtensions.SetupMongoDd(MongoDbContainer.GetConnectionString());
        Client = mongoClient;
        Database = mongoDatabase;
    }
    
    public void Clean()
    {
        var collections = Database.ListCollectionNames();
        while (collections.MoveNext())
        {
            var collection = collections.Current;
            var colection = collection.FirstOrDefault();
            
            if (colection is not null)
            {
                Database.DropCollection(colection);
            }
        }
    }

    public async Task DisposeAsync()
    {
        await MongoDbContainer.DisposeAsync();
    }
}
