using AshWatch.Application.Dtos;
using AshWatch.Application.Common;
using AshWatch.Domain.Entities;

namespace AshWatch.Application.Contracts;

public interface ILogService
{
    Task<DefaultResponse<Log>> LogAsync(CreateLogRequest request);
    Task<DefaultResponse<List<Log>>> LogBatchAsync(List<CreateLogRequest> requests);
    Task<DefaultResponse<Log>> GetLogByIdAsync(int id, int tenantId, int projectId);
    Task<DefaultResponse<List<Log>>> GetAllLogsAsync(GetLogsFilterRequest request);
}
