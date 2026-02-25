using AshWatch.Domain.Entities;

namespace AshWatch.Domain.Repositories;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<IEnumerable<Project>> GetAllByTenantAsync(Guid tenantId);
    Task<Project?> GetByIdAsync(Guid id, Guid tenantId);
    Task<bool> TenantExistsAsync(Guid tenantId);
    Task<bool> ExistsByNameAsync(Guid tenantId, string normalizedName, Guid? excludeId = null);
}
