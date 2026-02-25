using AshWatch.Domain.Entities;
using AshWatch.Domain.Repositories;
using AshWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AshWatch.Infrastructure.Repositories;

public class TenantRepository : GenericRepository<Tenant>, ITenantRepository
{
    private readonly ApplicationDbContext _dataContext;

    public TenantRepository(ApplicationDbContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null)
    {
        var query = _dataContext.Tenants.Where(x => x.Name.ToLower() == normalizedName.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public Task<bool> HasProjectsAsync(Guid tenantId)
    {
        return _dataContext.Projects.AnyAsync(x => x.TenantId == tenantId);
    }
}
