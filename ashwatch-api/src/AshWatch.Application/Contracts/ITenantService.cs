using AshWatch.Application.Common;
using AshWatch.Application.Dtos;
using AshWatch.Domain.Entities;

namespace AshWatch.Application.Contracts;

public interface ITenantService
{
    Task<DefaultResponse<Tenant>> CreateAsync(CreateTenantRequest request);
    Task<DefaultResponse<List<Tenant>>> GetAllAsync();
    Task<DefaultResponse<Tenant>> GetByIdAsync(Guid id);
    Task<DefaultResponse<Tenant>> UpdateAsync(Guid id, UpdateTenantRequest request);
    Task<DefaultResponse<bool>> DeleteAsync(Guid id);
}
