using AshWatch.Application.Common;
using AshWatch.Application.Dtos;
using AshWatch.Domain.Entities;

namespace AshWatch.Application.Contracts;

public interface ITenantService
{
    Task<DefaultResponse<Tenant>> CreateAsync(CreateTenantRequest request);
    Task<DefaultResponse<List<Tenant>>> GetAllAsync();
    Task<DefaultResponse<Tenant>> GetByIdAsync(int id);
    Task<DefaultResponse<Tenant>> UpdateAsync(int id, UpdateTenantRequest request);
    Task<DefaultResponse<bool>> DeleteAsync(int id);
}
