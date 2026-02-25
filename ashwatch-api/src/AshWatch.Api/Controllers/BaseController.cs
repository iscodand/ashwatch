using AshWatch.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace AshWatch.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult FromService<T>(DefaultResponse<T> response)
    {
        if (response.Success)
        {
            return Ok(response);
        }

        if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(response);
        }

        return BadRequest(response);
    }
}
