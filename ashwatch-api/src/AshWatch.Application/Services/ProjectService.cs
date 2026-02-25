using AshWatch.Application.Common;
using AshWatch.Application.Contracts;
using AshWatch.Application.Dtos;
using AshWatch.Domain.Entities;
using AshWatch.Domain.Repositories;

namespace AshWatch.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<DefaultResponse<Project>> CreateAsync(CreateProjectRequest request)
    {
        var errors = RequestValidator.Validate(request);
        if (request.TenantId == Guid.Empty)
        {
            errors.Add("TenantId must be a valid GUID.");
        }

        if (request.AuthorId == Guid.Empty)
        {
            errors.Add("AuthorId must be a valid GUID.");
        }

        if (errors.Count > 0)
        {
            return DefaultResponse<Project>.Fail("Validation failed.", errors.ToArray());
        }

        var tenantExists = await _projectRepository.TenantExistsAsync(request.TenantId);
        if (!tenantExists)
        {
            return DefaultResponse<Project>.Fail("Validation failed.", "Tenant does not exist.");
        }

        var normalizedName = NormalizeName(request.Name);
        var projectNameInUse = await _projectRepository.ExistsByNameAsync(request.TenantId, normalizedName);
        if (projectNameInUse)
        {
            return DefaultResponse<Project>.Fail(
                "Validation failed.",
                "A project with this name already exists for this tenant."
            );
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            AuthorId = request.AuthorId,
            Name = normalizedName,
            Description = request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);
        return DefaultResponse<Project>.Ok(project, "Project created successfully.");
    }

    public async Task<DefaultResponse<List<Project>>> GetAllAsync(Guid? tenantId)
    {
        if (tenantId.HasValue)
        {
            if (tenantId.Value == Guid.Empty)
            {
                return DefaultResponse<List<Project>>.Fail("Validation failed.", "TenantId must be a valid GUID.");
            }

            var tenantExists = await _projectRepository.TenantExistsAsync(tenantId.Value);
            if (!tenantExists)
            {
                return DefaultResponse<List<Project>>.Fail("Validation failed.", "Tenant does not exist.");
            }

            var tenantProjects = await _projectRepository.GetAllByTenantAsync(tenantId.Value);
            return DefaultResponse<List<Project>>.Ok(tenantProjects.ToList());
        }

        var projects = await _projectRepository.GetAllAsync();
        return DefaultResponse<List<Project>>.Ok(projects.OrderBy(x => x.Name).ToList());
    }

    public async Task<DefaultResponse<Project>> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (id == Guid.Empty || tenantId == Guid.Empty)
        {
            return DefaultResponse<Project>.Fail("Validation failed.", "Id and tenantId must be valid GUIDs.");
        }

        var project = await _projectRepository.GetByIdAsync(id, tenantId);
        if (project is null)
        {
            return DefaultResponse<Project>.Fail("Project not found.");
        }

        return DefaultResponse<Project>.Ok(project);
    }

    public async Task<DefaultResponse<Project>> UpdateAsync(Guid id, UpdateProjectRequest request)
    {
        if (id == Guid.Empty)
        {
            return DefaultResponse<Project>.Fail("Validation failed.", "Id must be a valid GUID.");
        }

        var errors = RequestValidator.Validate(request);
        if (request.TenantId == Guid.Empty)
        {
            errors.Add("TenantId must be a valid GUID.");
        }

        if (request.AuthorId == Guid.Empty)
        {
            errors.Add("AuthorId must be a valid GUID.");
        }

        if (errors.Count > 0)
        {
            return DefaultResponse<Project>.Fail("Validation failed.", errors.ToArray());
        }

        Project currentProject;
        try
        {
            currentProject = await _projectRepository.GetByIdAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return DefaultResponse<Project>.Fail("Project not found.");
        }

        var tenantExists = await _projectRepository.TenantExistsAsync(request.TenantId);
        if (!tenantExists)
        {
            return DefaultResponse<Project>.Fail("Validation failed.", "Tenant does not exist.");
        }

        var normalizedName = NormalizeName(request.Name);
        var projectNameInUse = await _projectRepository.ExistsByNameAsync(request.TenantId, normalizedName, id);
        if (projectNameInUse)
        {
            return DefaultResponse<Project>.Fail(
                "Validation failed.",
                "A project with this name already exists for this tenant."
            );
        }

        currentProject.TenantId = request.TenantId;
        currentProject.AuthorId = request.AuthorId;
        currentProject.Name = normalizedName;
        currentProject.Description = request.Description.Trim();

        await _projectRepository.UpdateAsync(currentProject);
        return DefaultResponse<Project>.Ok(currentProject, "Project updated successfully.");
    }

    public async Task<DefaultResponse<bool>> DeleteAsync(Guid id, Guid tenantId)
    {
        if (id == Guid.Empty || tenantId == Guid.Empty)
        {
            return DefaultResponse<bool>.Fail("Validation failed.", "Id and tenantId must be valid GUIDs.");
        }

        var project = await _projectRepository.GetByIdAsync(id, tenantId);
        if (project is null)
        {
            return DefaultResponse<bool>.Fail("Project not found.");
        }

        await _projectRepository.DeleteAsync(id);
        return DefaultResponse<bool>.Ok(true, "Project deleted successfully.");
    }

    private static string NormalizeName(string name)
    {
        return name.Trim();
    }
}
