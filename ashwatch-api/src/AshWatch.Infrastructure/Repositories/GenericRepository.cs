using AshWatch.Domain.Repositories;
using AshWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AshWatch.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _dataContext;
    private readonly DbSet<T> _set;

    public GenericRepository(ApplicationDbContext dataContext)
    {
        EnsureEntityHasGuidId();
        _dataContext = dataContext;
        _set = dataContext.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _set.AsNoTracking().ToListAsync();
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        var entity = await _set.AsNoTracking().FirstOrDefaultAsync(x => EF.Property<Guid>(x, "Id") == id);
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

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _set.FirstOrDefaultAsync(x => EF.Property<Guid>(x, "Id") == id);
        if (entity is null)
        {
            return;
        }

        _set.Remove(entity);
        await _dataContext.SaveChangesAsync();
    }

    private static void EnsureEntityHasGuidId()
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty is null || idProperty.PropertyType != typeof(Guid))
        {
            throw new InvalidOperationException(
                $"Entity type {typeof(T).Name} must have a Guid Id property."
            );
        }
    }
}
