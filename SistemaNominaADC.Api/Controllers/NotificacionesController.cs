using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;
using System.Security.Claims;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _service;

    public NotificacionesController(INotificacionService service)
    {
        _service = service;
    }

    [HttpGet("mias")]
    public async Task<IActionResult> Mias([FromQuery] bool soloPendientes = false, [FromQuery] int max = 50)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new BusinessException("No se pudo identificar al usuario autenticado.");

        return Ok(await _service.ListarMisNotificacionesAsync(userId, soloPendientes, max));
    }

    [HttpPatch("{id:int}/leer")]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        if (id <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id inválido."] }));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new BusinessException("No se pudo identificar al usuario autenticado.");

        await _service.MarcarLeidaAsync(id, userId);
        return NoContent();
    }

    [HttpPatch("leer-todas")]
    public async Task<IActionResult> MarcarTodasLeidas()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new BusinessException("No se pudo identificar al usuario autenticado.");

        await _service.MarcarTodasLeidasAsync(userId);
        return NoContent();
    }
}
