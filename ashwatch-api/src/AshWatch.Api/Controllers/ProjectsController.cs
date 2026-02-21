using AshWatch.Application.Contracts;
using AshWatch.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AshWatch.Api.Controllers;

[Route("projects")]
public class ProjectsController : ApiResponseControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var response = await _projectService.CreateAsync(request);
        return FromService(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? tenantId)
    {
        var response = await _projectService.GetAllAsync(tenantId);
        return FromService(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] int tenantId)
    {
        var response = await _projectService.GetByIdAsync(id, tenantId);
        return FromService(response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateProjectRequest request)
    {
        var response = await _projectService.UpdateAsync(id, request);
        return FromService(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, [FromQuery] int tenantId)
    {
        var response = await _projectService.DeleteAsync(id, tenantId);
        return FromService(response);
    }
}
