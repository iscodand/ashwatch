using AshWatch.Application.Contracts;
using AshWatch.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AshWatch.Api.Controllers;

[Route("tenants")]
public class TenantsController : ApiResponseControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        var response = await _tenantService.CreateAsync(request);
        return FromService(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _tenantService.GetAllAsync();
        return FromService(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var response = await _tenantService.GetByIdAsync(id);
        return FromService(response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateTenantRequest request)
    {
        var response = await _tenantService.UpdateAsync(id, request);
        return FromService(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var response = await _tenantService.DeleteAsync(id);
        return FromService(response);
    }
}
