using Microsoft.AspNetCore.Mvc;
using EnerGym.Infrastructure;

namespace EnerGym.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        // Devuelve datos directamente (para compatibilidad con frontend legacy)
        protected IActionResult OkData<T>(T data)
        {
            return Ok(data);
        }

        // Devuelve mensaje envuelto en ApiResponse
        protected IActionResult OkMessage(string? message = null)
        {
            return Ok(ApiResponse.Ok(message));
        }

        protected IActionResult BadRequestResponse(string error)
        {
            return BadRequest(ApiResponse.Fail(error));
        }

        protected IActionResult NotFoundResponse(string error)
        {
            return NotFound(ApiResponse.Fail(error));
        }

        protected IActionResult UnauthorizedResponse(string error)
        {
            return Unauthorized(ApiResponse.Fail(error));
        }

        protected IActionResult ConflictResponse(string error)
        {
            return Conflict(ApiResponse.Fail(error));
        }
    }
}
