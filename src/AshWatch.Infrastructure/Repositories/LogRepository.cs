using AshWatch.Domain.Entities;
using AshWatch.Domain.Repositories;
using AshWatch.Infrastructure.Data;
using MongoDB.Driver;

namespace AshWatch.Infrastructure.Repositories;

public class LogRepository : GenericRepository<Log>, ILogRepository
{
    public LogRepository(DataContext dataContext) : base(dataContext) { }

    public async Task<Log?> GetByIdAsync(int id, int tenantId, int projectId)
    {
        var filter = Builders<Log>.Filter.Eq(x => x.Id, id)
            & Builders<Log>.Filter.Eq(x => x.TenantId, tenantId)
            & Builders<Log>.Filter.Eq(x => x.ProjectId, projectId);

        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Log>> GetAllAsync(int tenantId, int projectId, DateTime? startDate, DateTime? endDate)
    {
        var filter = Builders<Log>.Filter.Eq(x => x.TenantId, tenantId)
            & Builders<Log>.Filter.Eq(x => x.ProjectId, projectId);

        if (startDate.HasValue)
        {
            filter &= Builders<Log>.Filter.Gte(x => x.Timestamp, startDate.Value);
        }

        if (endDate.HasValue)
        {
            filter &= Builders<Log>.Filter.Lte(x => x.Timestamp, endDate.Value);
        }

        return await Collection.Find(filter).SortByDescending(x => x.Timestamp).ToListAsync();
    }
}
