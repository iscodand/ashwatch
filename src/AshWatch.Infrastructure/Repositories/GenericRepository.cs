using AshWatch.Domain.Repositories;
using AshWatch.Infrastructure.Data;
using MongoDB.Driver;

namespace AshWatch.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly IMongoCollection<T> Collection;

    public GenericRepository(DataContext dataContext)
    {
        Collection = dataContext.GetCollection<T>();
    }

    public Task AddAsync(T entity)
    {
        return Collection.InsertOneAsync(entity);
    }

    public Task DeleteAsync(int id)
    {
        return Collection.DeleteOneAsync(Builders<T>.Filter.Eq("Id", id));
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Collection.Find(Builders<T>.Filter.Empty).ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        var entity = await Collection.Find(Builders<T>.Filter.Eq("Id", id)).FirstOrDefaultAsync();
        return entity ?? throw new KeyNotFoundException($"Entity {typeof(T).Name} with Id {id} was not found.");
    }

    public Task UpdateAsync(T entity)
    {
        var id = GetEntityId(entity);
        return Collection.ReplaceOneAsync(Builders<T>.Filter.Eq("Id", id), entity);
    }

    private static int GetEntityId(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty is null || idProperty.PropertyType != typeof(int))
        {
            throw new InvalidOperationException(
                $"Entity type {typeof(T).Name} must have an int Id property."
            );
        }

        var value = idProperty.GetValue(entity);
        return value is int id
            ? id
            : throw new InvalidOperationException($"Entity type {typeof(T).Name} has an invalid Id value.");
    }
}
