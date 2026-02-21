using AshWatch.Application.Contracts;
using AshWatch.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AshWatch.Api.Controllers;

[Route("logs")]
public class LogsController : ApiResponseControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLogRequest request)
    {
        var response = await _logService.LogAsync(request);
        return FromService(response);
    }

    [HttpPost("batch")]
    public async Task<IActionResult> CreateBatch([FromBody] List<CreateLogRequest> requests)
    {
        var response = await _logService.LogBatchAsync(requests);
        return FromService(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] int tenantId, [FromQuery] int projectId)
    {
        var response = await _logService.GetLogByIdAsync(id, tenantId, projectId);
        return FromService(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetLogsFilterRequest request)
    {
        var response = await _logService.GetAllLogsAsync(request);
        return FromService(response);
    }
}
