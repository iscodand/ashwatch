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

        _database = mongoClient.GetDatabase("ashwatch_db");
    }

    public IMongoCollection<Log> Logs =>
        _database.GetCollection<Log>("logs");

    public IMongoCollection<Tenant> Tenants =>
        _database.GetCollection<Tenant>("tenants");

    public IMongoCollection<Project> Projects =>
        _database.GetCollection<Project>("projects");
}
