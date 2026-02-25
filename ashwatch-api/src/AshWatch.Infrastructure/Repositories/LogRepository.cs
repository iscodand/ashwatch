using AshWatch.Domain.Entities;
using AshWatch.Domain.Repositories;
using AshWatch.Infrastructure.Data;
namespace AshWatch.Infrastructure.Repositories;

public class LogRepository : GenericRepository<Log>, ILogRepository
{
    public LogRepository(ApplicationDbContext dataContext) : base(dataContext) { }
}
