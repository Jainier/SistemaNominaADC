using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Interfaces;
using System.Security.Claims;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PermisosController : ControllerBase
{
    private readonly IPermisoService _service;
    private readonly ApplicationDbContext _context;
    private readonly ISolicitudesAuthorizationService _authz;

    public PermisosController(IPermisoService service, ApplicationDbContext context, ISolicitudesAuthorizationService authz)
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
            return Ok(await _service.HistorialPorEmpleadosAsync(empleadosGestionables, idEmpleado, fechaDesde, fechaHasta, idEstado));
        }

        var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
        if (!idEmpleadoActual.HasValue)
            return Forbid();

        return Ok(await _service.HistorialAsync(idEmpleadoActual.Value, fechaDesde, fechaHasta, idEstado));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] PermisoCreateDTO dto)
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
    public async Task<IActionResult> Actualizar(int id, [FromBody] PermisoCreateDTO dto)
    {
        if (id <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var permiso = await _context.Permisos.FirstOrDefaultAsync(x => x.IdPermiso == id);
        if (permiso is null)
            return NotFound("Permiso no encontrado.");
        if (!permiso.IdEmpleado.HasValue)
            return BadRequest("El permiso no tiene empleado asociado.");

        dto.IdEmpleado = permiso.IdEmpleado.Value;

        var esGlobal = await _authz.EsAprobadorGlobalAsync(User);
        var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
        var empleadosGestionables = await _authz.ObtenerEmpleadosGestionablesAsync(User);

        var puedeEditarExistente = esGlobal
            || (idEmpleadoActual.HasValue && idEmpleadoActual.Value == permiso.IdEmpleado.Value)
            || empleadosGestionables.Contains(permiso.IdEmpleado.Value);
        if (!puedeEditarExistente)
            return Forbid();

        var actualizado = await _service.ActualizarPendienteAsync(id, dto, User.FindFirstValue(ClaimTypes.NameIdentifier));
        return Ok(actualizado);
    }

    [HttpGet("tipos-disponibles")]
    [Obsolete("Deprecated: usar GET api/TipoPermiso para catálogos de combo.")]
    public async Task<IActionResult> TiposDisponibles()
    {
        var tipos = await _context.TipoPermisos
            .AsNoTracking()
            .Select(t => new
            {
                t.IdTipoPermiso,
                Nombre = EF.Property<string?>(t, nameof(TipoPermiso.Nombre)),
                IdEstado = EF.Property<int?>(t, nameof(TipoPermiso.IdEstado)),
                EstadoNombre = t.Estado != null ? t.Estado.Nombre : null,
                EstadoActivo = t.Estado != null ? t.Estado.EstadoActivo : null
            })
            .Where(t =>
                !string.IsNullOrWhiteSpace(t.Nombre) &&
                t.IdEstado.HasValue &&
                ((t.EstadoActivo ?? false) || string.Equals(t.EstadoNombre, "Activo")))
            .OrderBy(t => t.Nombre)
            .ToListAsync();

        return Ok(tipos.Select(t => new TipoPermiso
        {
            IdTipoPermiso = t.IdTipoPermiso,
            Nombre = t.Nombre!,
            IdEstado = t.IdEstado!.Value,
            Estado = new Estado
            {
                IdEstado = t.IdEstado.Value,
                Nombre = t.EstadoNombre,
                EstadoActivo = t.EstadoActivo
            }
        }));
    }

    [HttpPatch("{id:int}/aprobar")]
    public async Task<IActionResult> Aprobar(int id, [FromBody] SolicitudDecisionDTO? dto)
    {
        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));

        var permiso = await _context.Permisos.FirstOrDefaultAsync(x => x.IdPermiso == id);
        if (permiso is null) return NotFound("Permiso no encontrado.");
        if (!permiso.IdEmpleado.HasValue) return BadRequest("El permiso no tiene empleado asociado.");

        if (!await _authz.PuedeAprobarEmpleadoAsync(User, permiso.IdEmpleado.Value))
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

        var permiso = await _context.Permisos.FirstOrDefaultAsync(x => x.IdPermiso == id);
        if (permiso is null) return NotFound("Permiso no encontrado.");
        if (!permiso.IdEmpleado.HasValue) return BadRequest("El permiso no tiene empleado asociado.");

        if (!await _authz.PuedeAprobarEmpleadoAsync(User, permiso.IdEmpleado.Value))
            return Forbid();

        var actualizado = await _service.RechazarAsync(id, dto.Comentario, User.FindFirstValue(ClaimTypes.NameIdentifier));
        return Ok(actualizado);
    }

    [HttpGet("{id:int}/acciones")]
    public async Task<IActionResult> AccionesDisponibles(int id)
    {
        if (id <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));

        var permiso = await _context.Permisos.AsNoTracking().FirstOrDefaultAsync(x => x.IdPermiso == id);
        if (permiso is null)
            return NotFound("Permiso no encontrado.");
        if (!permiso.IdEmpleado.HasValue)
            return BadRequest("El permiso no tiene empleado asociado.");

        if (await _authz.EsAprobadorGlobalAsync(User))
            return Ok(await _service.ObtenerAccionesDisponibles(id, ObtenerRolesUsuario()));

        var empleadosGestionables = await _authz.ObtenerEmpleadosGestionablesAsync(User);
        var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);

        var puedeVer = (idEmpleadoActual.HasValue && idEmpleadoActual.Value == permiso.IdEmpleado.Value)
            || empleadosGestionables.Contains(permiso.IdEmpleado.Value);

        if (!puedeVer)
            return Forbid();

        return Ok(await _service.ObtenerAccionesDisponibles(id, ObtenerRolesUsuario()));
    }

    [HttpPatch("{id:int}/accion")]
    public async Task<IActionResult> EjecutarAccion(int id, [FromBody] EjecutarAccionWorkflowDTO dto)
    {
        if (id <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var permiso = await _context.Permisos.AsNoTracking().FirstOrDefaultAsync(x => x.IdPermiso == id);
        if (permiso is null)
            return NotFound("Permiso no encontrado.");
        if (!permiso.IdEmpleado.HasValue)
            return BadRequest("El permiso no tiene empleado asociado.");

        if (!await _authz.PuedeAprobarEmpleadoAsync(User, permiso.IdEmpleado.Value))
            return Forbid();

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
