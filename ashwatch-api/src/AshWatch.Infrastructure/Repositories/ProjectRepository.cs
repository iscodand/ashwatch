using AshWatch.Domain.Entities;
using AshWatch.Domain.Repositories;
using AshWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AshWatch.Infrastructure.Repositories;

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    private readonly ApplicationDbContext _dataContext;

    public ProjectRepository(ApplicationDbContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<IEnumerable<Project>> GetAllByTenantAsync(int tenantId)
    {
        return await _dataContext.Projects
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public Task<Project?> GetByIdAsync(int id, int tenantId)
    {
        return _dataContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public Task<bool> TenantExistsAsync(int tenantId)
    {
        return _dataContext.Tenants.AnyAsync(x => x.Id == tenantId);
    }

    public async Task<bool> ExistsByNameAsync(int tenantId, string normalizedName, int? excludeId = null)
    {
        var query = _dataContext.Projects.Where(
            x => x.TenantId == tenantId && x.Name.ToLower() == normalizedName.ToLower()
        );

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
