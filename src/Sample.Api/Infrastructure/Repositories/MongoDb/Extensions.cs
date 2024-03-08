using MongoDB.Driver;
using Sample.Api.Core.Model;
using Sample.Api.Infrastructure.EfCore;

namespace Sample.Api.Infrastructure.Repositories.MongoDb;

public static class MongoDbExtensions
{
    public static IMongoCollection<Order> Orders(this IMongoDatabase db)
    {
        return db.GetCollection<Order>("orders");
    }
    
    public static IMongoCollection<OutBox> OutBox(this IMongoDatabase db)
    {
        return db.GetCollection<OutBox>("outBox");
    }
    
    public static IMongoCollection<Order> Products(this IMongoDatabase db)
    {
        return db.GetCollection<Order>("products");
    }
}
