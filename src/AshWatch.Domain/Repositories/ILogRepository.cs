using AshWatch.Domain.Entities;

namespace AshWatch.Domain.Repositories;

public interface ILogRepository : IGenericRepository<Log>
{
    Task<Log?> GetByIdAsync(int id, int tenantId, int projectId);
    Task<IEnumerable<Log>> GetAllAsync(int tenantId, int projectId, DateTime? startDate, DateTime? endDate);
}
