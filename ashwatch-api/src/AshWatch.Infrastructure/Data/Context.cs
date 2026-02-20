using AshWatch.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace AshWatch.Infrastructure.Data;

public class DataContext
{
    private readonly IMongoDatabase _database;

    public DataContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb");
        var mongoClient = new MongoClient(connectionString);

        _database = mongoClient.GetDatabase("ash_logging_dev");
    }

    public IMongoCollection<Log> Logs => GetCollection<Log>();

    public IMongoCollection<Tenant> Tenants => GetCollection<Tenant>();

    public IMongoCollection<Project> Projects => GetCollection<Project>();

    public IMongoCollection<T> GetCollection<T>() where T : class
    {
        var collectionName = $"{typeof(T).Name.ToLowerInvariant()}s";
        return _database.GetCollection<T>(collectionName);
    }
}
