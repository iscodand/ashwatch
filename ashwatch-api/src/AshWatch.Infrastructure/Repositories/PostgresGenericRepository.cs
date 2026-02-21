using AshWatch.Domain.Repositories;
using AshWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AshWatch.Infrastructure.Repositories;

public class PostgresGenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly PostgresDataContext _dataContext;
    private readonly DbSet<T> _set;

    public PostgresGenericRepository(PostgresDataContext dataContext)
    {
        EnsureEntityHasIntId();
        _dataContext = dataContext;
        _set = dataContext.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _set.AsNoTracking().ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        var entity = await _set.AsNoTracking().FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id);
        return entity ?? throw new KeyNotFoundException($"Entity {typeof(T).Name} with Id {id} was not found.");
    }

    public async Task AddAsync(T entity)
    {
        await _set.AddAsync(entity);
        await _dataContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _set.Update(entity);
        await _dataContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _set.FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id);
        if (entity is null)
        {
            return;
        }

        _set.Remove(entity);
        await _dataContext.SaveChangesAsync();
    }

    private static void EnsureEntityHasIntId()
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty is null || idProperty.PropertyType != typeof(int))
        {
            throw new InvalidOperationException(
                $"Entity type {typeof(T).Name} must have an int Id property."
            );
        }
    }
}
