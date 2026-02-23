using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;
using System.Security.Claims;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AsistenciaController : ControllerBase
{
    private readonly IAsistenciaService _service;
    private readonly ApplicationDbContext _context;

    public AsistenciaController(IAsistenciaService service, ApplicationDbContext context)
    {
        _service = service;
        _context = context;
    }

    [HttpGet("mi-empleado")]
    public async Task<IActionResult> MiEmpleado()
    {
        try
        {
            var empleado = await ObtenerEmpleadoActualAsync();
            return Ok(empleado);
        }
        catch (BusinessException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Regla de negocio", detail: ex.Message);
        }
        catch (NotFoundException ex)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Recurso no encontrado", detail: ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Historial([FromQuery] int? idEmpleado, [FromQuery] DateTime? fechaDesde, [FromQuery] DateTime? fechaHasta)
    {
        try
        {
            if (!EsAdministrador())
            {
                var empleadoActual = await ObtenerEmpleadoActualAsync();
                idEmpleado = empleadoActual.IdEmpleado;
            }

            if (idEmpleado.HasValue && idEmpleado.Value <= 0)
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["idEmpleado"] = ["Id inválido."] }));

            if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde.Value.Date > fechaHasta.Value.Date)
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["fechaRango"] = ["La fecha inicial no puede ser mayor que la fecha final."] }));

            return Ok(await _service.Historial(idEmpleado, fechaDesde, fechaHasta));
        }
        catch (BusinessException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Regla de negocio", detail: ex.Message);
        }
        catch (NotFoundException ex)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Recurso no encontrado", detail: ex.Message);
        }
    }

    [HttpPost("entrada")]
    public async Task<IActionResult> RegistrarEntrada([FromBody] AsistenciaMarcaDTO dto)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!EsAdministrador())
            {
                var empleadoActual = await ObtenerEmpleadoActualAsync();
                dto.IdEmpleado = empleadoActual.IdEmpleado;
            }
            var asistencia = await _service.RegistrarEntrada(dto);
            return Ok(asistencia);
        }
        catch (BusinessException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Regla de negocio", detail: ex.Message);
        }
        catch (NotFoundException ex)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Recurso no encontrado", detail: ex.Message);
        }
    }

    [HttpPost("salida")]
    public async Task<IActionResult> RegistrarSalida([FromBody] AsistenciaMarcaDTO dto)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!EsAdministrador())
            {
                var empleadoActual = await ObtenerEmpleadoActualAsync();
                dto.IdEmpleado = empleadoActual.IdEmpleado;
            }
            var asistencia = await _service.RegistrarSalida(dto);
            return Ok(asistencia);
        }
        catch (BusinessException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Regla de negocio", detail: ex.Message);
        }
        catch (NotFoundException ex)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Recurso no encontrado", detail: ex.Message);
        }
    }

    private bool EsAdministrador() =>
        User.IsInRole("Admin") || User.IsInRole("Administrador") || User.IsInRole("ADMINISTRADOR");

    private async Task<SistemaNominaADC.Entidades.Empleado> ObtenerEmpleadoActualAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new BusinessException("No se pudo identificar el usuario autenticado.");

        return await _context.Empleados
            .Include(e => e.Puesto)
            .Include(e => e.Estado)
            .FirstOrDefaultAsync(e => e.IdentityUserId == userId)
            ?? throw new NotFoundException("No se encontró un empleado asociado al usuario autenticado.");
    }
}
