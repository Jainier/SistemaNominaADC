using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Interfaces;
using SistemaNominaADC.Negocio.Servicios;
using System.Security.Claims;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SolicitudesVacacionesController : ControllerBase
{
    private readonly ISolicitudVacacionesService _service;
    private readonly ApplicationDbContext _context;
    private readonly ISolicitudesAuthorizationService _authz;

    public SolicitudesVacacionesController(ISolicitudVacacionesService service, ApplicationDbContext context, ISolicitudesAuthorizationService authz)
    {
        _service = service;
        _context = context;
        _authz = authz;
    }

    [HttpGet]
    public async Task<IActionResult> Historial([FromQuery] int? idEmpleado, [FromQuery] DateTime? fechaDesde, [FromQuery] DateTime? fechaHasta, [FromQuery] int? idEstado)
    {
        if (await _authz.EsAprobadorGlobalAsync(User))
            return Ok(await _service.HistorialAsync(idEmpleado, fechaDesde, fechaHasta, idEstado));

        var empleadosGestionables = await _authz.ObtenerEmpleadosGestionablesAsync(User);
        if (empleadosGestionables.Count > 0)
        {
            var lista = await _service.HistorialAsync(idEmpleado, fechaDesde, fechaHasta, idEstado);
            return Ok(lista.Where(x => x.IdEmpleado.HasValue && empleadosGestionables.Contains(x.IdEmpleado.Value)).ToList());
        }

        var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
        if (!idEmpleadoActual.HasValue)
            return Forbid();

        return Ok(await _service.HistorialAsync(idEmpleadoActual.Value, fechaDesde, fechaHasta, idEstado));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] SolicitudVacacionesCreateDTO dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
        if (await _authz.EsAprobadorGlobalAsync(User))
        {
            // Puede crear para cualquier empleado.
        }
        else if (idEmpleadoActual.HasValue)
        {
            if (dto.IdEmpleado <= 0)
            {
                dto.IdEmpleado = idEmpleadoActual.Value;
            }
            else
            {
                var empleadosGestionables = await _authz.ObtenerEmpleadosGestionablesAsync(User);
                if (!empleadosGestionables.Contains(dto.IdEmpleado))
                    return Forbid();
            }
        }
        else
        {
            return Forbid();
        }

        var creado = await _service.CrearAsync(dto, User.FindFirstValue(ClaimTypes.NameIdentifier));
        return CreatedAtAction(nameof(Historial), new { idEmpleado = creado.IdEmpleado }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] SolicitudVacacionesCreateDTO dto)
    {
        if (id <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var solicitud = await _context.SolicitudesVacaciones.FirstOrDefaultAsync(x => x.IdSolicitudVacaciones == id);
        if (solicitud is null)
            return NotFound("Solicitud de vacaciones no encontrada.");
        if (!solicitud.IdEmpleado.HasValue)
            return BadRequest("La solicitud no tiene empleado asociado.");

        dto.IdEmpleado = solicitud.IdEmpleado.Value;

        var esGlobal = await _authz.EsAprobadorGlobalAsync(User);
        var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
        var empleadosGestionables = await _authz.ObtenerEmpleadosGestionablesAsync(User);

        var puedeEditarExistente = esGlobal
            || (idEmpleadoActual.HasValue && idEmpleadoActual.Value == solicitud.IdEmpleado.Value)
            || empleadosGestionables.Contains(solicitud.IdEmpleado.Value);
        if (!puedeEditarExistente)
            return Forbid();

        if (!esGlobal && idEmpleadoActual.HasValue)
        {
            if (dto.IdEmpleado <= 0)
            {
                dto.IdEmpleado = solicitud.IdEmpleado.Value;
            }
            else if (dto.IdEmpleado != idEmpleadoActual.Value && !empleadosGestionables.Contains(dto.IdEmpleado))
            {
                return Forbid();
            }
        }
        else
        {
            return Forbid();
        }

        var actualizado = await _service.ActualizarPendienteAsync(id, dto, User.FindFirstValue(ClaimTypes.NameIdentifier));
        return Ok(actualizado);
    }

    [HttpGet("estados-disponibles")]
    public async Task<IActionResult> EstadosDisponibles()
    {
        var estados = await _context.Estados
            .AsNoTracking()
            .Where(e => e.EstadoActivo ?? false)
            .OrderBy(e => e.Nombre)
            .Select(e => new Estado
            {
                IdEstado = e.IdEstado,
                Codigo = e.Codigo,
                Nombre = e.Nombre ?? string.Empty,
                Descripcion = e.Descripcion ?? string.Empty,
                EstadoActivo = e.EstadoActivo
            })
            .ToListAsync();

        return Ok(estados);
    }

    [HttpGet("saldo/{idEmpleado:int}")]
    public async Task<IActionResult> SaldoVacaciones(int idEmpleado)
    {
        if (idEmpleado <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["idEmpleado"] = ["Id invalido."] }));

        var esGlobal = await _authz.EsAprobadorGlobalAsync(User);
        if (!esGlobal)
        {
            var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
            var empleadosGestionables = await _authz.ObtenerEmpleadosGestionablesAsync(User);
            var autorizado = (idEmpleadoActual.HasValue && idEmpleadoActual.Value == idEmpleado) || empleadosGestionables.Contains(idEmpleado);
            if (!autorizado)
                return Forbid();
        }

        var saldo = await _context.Vacaciones
            .AsNoTracking()
            .Where(v => v.IdEmpleado == idEmpleado)
            .Select(v => new { diasRestantes = v.DiasRestantes ?? 0 })
            .FirstOrDefaultAsync();

        return Ok(saldo ?? new { diasRestantes = 0 });
    }

    [HttpPatch("{id:int}/aprobar")]
    public async Task<IActionResult> Aprobar(int id, [FromBody] SolicitudDecisionDTO? dto)
    {
        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));

        var solicitud = await _context.SolicitudesVacaciones.FirstOrDefaultAsync(x => x.IdSolicitudVacaciones == id);
        if (solicitud is null) return NotFound("Solicitud de vacaciones no encontrada.");
        if (!solicitud.IdEmpleado.HasValue) return BadRequest("La solicitud no tiene empleado asociado.");

        if (!await _authz.PuedeAprobarEmpleadoAsync(User, solicitud.IdEmpleado.Value))
            return Forbid();

        var actualizado = await _service.AprobarAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier), dto?.Comentario);
        return Ok(actualizado);
    }

    [HttpPatch("{id:int}/rechazar")]
    public async Task<IActionResult> Rechazar(int id, [FromBody] SolicitudDecisionDTO dto)
    {
        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (string.IsNullOrWhiteSpace(dto.Comentario))
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["comentario"] = ["El motivo de rechazo es obligatorio."] }));

        var solicitud = await _context.SolicitudesVacaciones.FirstOrDefaultAsync(x => x.IdSolicitudVacaciones == id);
        if (solicitud is null) return NotFound("Solicitud de vacaciones no encontrada.");
        if (!solicitud.IdEmpleado.HasValue) return BadRequest("La solicitud no tiene empleado asociado.");

        if (!await _authz.PuedeAprobarEmpleadoAsync(User, solicitud.IdEmpleado.Value))
            return Forbid();

        var actualizado = await _service.RechazarAsync(id, dto.Comentario, User.FindFirstValue(ClaimTypes.NameIdentifier));
        return Ok(actualizado);
    }

    [HttpGet("{id:int}/acciones")]
    public async Task<IActionResult> AccionesDisponibles(int id)
    {
        if (id <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));

        return Ok(await _service.ObtenerAccionesDisponibles(id, ObtenerRolesUsuario()));
    }

    [HttpPatch("{id:int}/accion")]
    public async Task<IActionResult> EjecutarAccion(int id, [FromBody] EjecutarAccionWorkflowDTO dto)
    {
        if (id <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var solicitud = await _context.SolicitudesVacaciones.FirstOrDefaultAsync(x => x.IdSolicitudVacaciones == id);
        if (solicitud is null) return NotFound("Solicitud de vacaciones no encontrada.");
        if (!solicitud.IdEmpleado.HasValue) return BadRequest("La solicitud no tiene empleado asociado.");

        var esEditar = string.Equals(dto.Accion, WorkflowAcciones.Editar, StringComparison.OrdinalIgnoreCase);
        if (esEditar)
        {
            var esGlobal = await _authz.EsAprobadorGlobalAsync(User);
            var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
            var empleadosGestionables = await _authz.ObtenerEmpleadosGestionablesAsync(User);
            var puedeEditarExistente = esGlobal
                || (idEmpleadoActual.HasValue && idEmpleadoActual.Value == solicitud.IdEmpleado.Value)
                || empleadosGestionables.Contains(solicitud.IdEmpleado.Value);
            if (!puedeEditarExistente)
                return Forbid();
        }
        else if (!await _authz.PuedeAprobarEmpleadoAsync(User, solicitud.IdEmpleado.Value))
        {
            return Forbid();
        }

        var actualizado = await _service.EjecutarAccionAsync(
            id,
            dto.Accion,
            dto.Comentario,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            ObtenerRolesUsuario());
        return Ok(actualizado);
    }

    private List<string> ObtenerRolesUsuario() =>
        User.Claims
            .Where(c => c.Type == ClaimTypes.Role && !string.IsNullOrWhiteSpace(c.Value))
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}
