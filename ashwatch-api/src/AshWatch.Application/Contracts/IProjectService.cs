using AshWatch.Application.Common;
using AshWatch.Application.Dtos;
using AshWatch.Domain.Entities;

namespace AshWatch.Application.Contracts;

public interface IProjectService
{
    Task<DefaultResponse<Project>> CreateAsync(CreateProjectRequest request);
    Task<DefaultResponse<List<Project>>> GetAllAsync(Guid? tenantId);
    Task<DefaultResponse<Project>> GetByIdAsync(Guid id, Guid tenantId);
    Task<DefaultResponse<Project>> UpdateAsync(Guid id, UpdateProjectRequest request);
    Task<DefaultResponse<bool>> DeleteAsync(Guid id, Guid tenantId);
}
