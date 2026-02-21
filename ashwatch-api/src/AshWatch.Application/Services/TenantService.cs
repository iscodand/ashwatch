using AshWatch.Application.Common;
using AshWatch.Application.Contracts;
using AshWatch.Application.Dtos;
using AshWatch.Domain.Entities;
using AshWatch.Domain.Repositories;

namespace AshWatch.Application.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;

    public TenantService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<DefaultResponse<Tenant>> CreateAsync(CreateTenantRequest request)
    {
        var errors = RequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return DefaultResponse<Tenant>.Fail("Validation failed.", errors.ToArray());
        }

        var normalizedName = NormalizeName(request.Name);
        var alreadyExists = await _tenantRepository.ExistsByNameAsync(normalizedName);
        if (alreadyExists)
        {
            return DefaultResponse<Tenant>.Fail("Validation failed.", "A tenant with this name already exists.");
        }

        var currentTenants = await _tenantRepository.GetAllAsync();
        var nextId = currentTenants.Any() ? currentTenants.Max(x => x.Id) + 1 : 1;

        var tenant = new Tenant
        {
            Id = nextId,
            AuthorId = request.AuthorId,
            Name = normalizedName,
            Description = request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _tenantRepository.AddAsync(tenant);
        return DefaultResponse<Tenant>.Ok(tenant, "Tenant created successfully.");
    }

    public async Task<DefaultResponse<List<Tenant>>> GetAllAsync()
    {
        var tenants = await _tenantRepository.GetAllAsync();
        return DefaultResponse<List<Tenant>>.Ok(tenants.OrderBy(x => x.Name).ToList());
    }

    public async Task<DefaultResponse<Tenant>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return DefaultResponse<Tenant>.Fail("Validation failed.", "Id must be greater than zero.");
        }

        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            return DefaultResponse<Tenant>.Ok(tenant);
        }
        catch (KeyNotFoundException)
        {
            return DefaultResponse<Tenant>.Fail("Tenant not found.");
        }
    }

    public async Task<DefaultResponse<Tenant>> UpdateAsync(int id, UpdateTenantRequest request)
    {
        if (id <= 0)
        {
            return DefaultResponse<Tenant>.Fail("Validation failed.", "Id must be greater than zero.");
        }

        var errors = RequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return DefaultResponse<Tenant>.Fail("Validation failed.", errors.ToArray());
        }

        Tenant currentTenant;
        try
        {
            currentTenant = await _tenantRepository.GetByIdAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return DefaultResponse<Tenant>.Fail("Tenant not found.");
        }

        var normalizedName = NormalizeName(request.Name);
        var sameNameInUse = await _tenantRepository.ExistsByNameAsync(normalizedName, id);

        if (sameNameInUse)
        {
            return DefaultResponse<Tenant>.Fail("Validation failed.", "A tenant with this name already exists.");
        }

        currentTenant.AuthorId = request.AuthorId;
        currentTenant.Name = normalizedName;
        currentTenant.Description = request.Description.Trim();

        await _tenantRepository.UpdateAsync(currentTenant);
        return DefaultResponse<Tenant>.Ok(currentTenant, "Tenant updated successfully.");
    }

    public async Task<DefaultResponse<bool>> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return DefaultResponse<bool>.Fail("Validation failed.", "Id must be greater than zero.");
        }

        try
        {
            _ = await _tenantRepository.GetByIdAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return DefaultResponse<bool>.Fail("Tenant not found.");
        }

        var hasProjects = await _tenantRepository.HasProjectsAsync(id);
        if (hasProjects)
        {
            return DefaultResponse<bool>.Fail("Validation failed.", "Cannot delete tenant with existing projects.");
        }

        await _tenantRepository.DeleteAsync(id);
        return DefaultResponse<bool>.Ok(true, "Tenant deleted successfully.");
    }

    private static string NormalizeName(string name)
    {
        return name.Trim();
    }
}
