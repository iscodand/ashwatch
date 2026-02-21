using AshWatch.Application.Common;
using AshWatch.Application.Dtos;
using AshWatch.Domain.Entities;

namespace AshWatch.Application.Contracts;

public interface IProjectService
{
    Task<DefaultResponse<Project>> CreateAsync(CreateProjectRequest request);
    Task<DefaultResponse<List<Project>>> GetAllAsync(int? tenantId);
    Task<DefaultResponse<Project>> GetByIdAsync(int id, int tenantId);
    Task<DefaultResponse<Project>> UpdateAsync(int id, UpdateProjectRequest request);
    Task<DefaultResponse<bool>> DeleteAsync(int id, int tenantId);
}
