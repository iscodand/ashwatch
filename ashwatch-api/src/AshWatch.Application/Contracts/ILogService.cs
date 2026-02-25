using AshWatch.Application.Dtos;
using AshWatch.Application.Common;
using AshWatch.Domain.Entities;

namespace AshWatch.Application.Contracts;

public interface ILogService
{
    Task<DefaultResponse<Log>> LogAsync(CreateLogRequest request);
    Task<DefaultResponse<List<Log>>> LogBatchAsync(List<CreateLogRequest> requests);
}
