using AshWatch.Domain.Entities;

namespace AshWatch.Domain.Repositories;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<IEnumerable<Project>> GetAllByTenantAsync(int tenantId);
    Task<Project?> GetByIdAsync(int id, int tenantId);
    Task<bool> TenantExistsAsync(int tenantId);
    Task<bool> ExistsByNameAsync(int tenantId, string normalizedName, int? excludeId = null);
}
