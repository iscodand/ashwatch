using AshWatch.Domain.Entities;

namespace AshWatch.Domain.Repositories;

public interface ITenantRepository : IGenericRepository<Tenant>
{
    Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null);
    Task<bool> HasProjectsAsync(Guid tenantId);
}
