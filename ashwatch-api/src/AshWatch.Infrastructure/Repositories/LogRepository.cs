using AshWatch.Domain.Entities;
using AshWatch.Domain.Repositories;
using AshWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AshWatch.Infrastructure.Repositories;

public class LogRepository : GenericRepository<Log>, ILogRepository
{
    private readonly ApplicationDbContext _dataContext;

    public LogRepository(ApplicationDbContext dataContext) : base(dataContext)
    {
        _dataContext = dataContext;
    }
}
