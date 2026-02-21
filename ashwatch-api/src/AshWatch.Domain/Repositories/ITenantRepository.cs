using AshWatch.Domain.Entities;

namespace AshWatch.Domain.Repositories;

public interface ITenantRepository : IGenericRepository<Tenant>
{
    Task<bool> ExistsByNameAsync(string normalizedName, int? excludeId = null);
    Task<bool> HasProjectsAsync(int tenantId);
}
