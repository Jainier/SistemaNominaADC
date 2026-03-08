using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Interfaces;
using SistemaNominaADC.Negocio.Servicios;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class IncapacidadesController : ControllerBase
{
    private readonly IIncapacidadService _service;
    private readonly ApplicationDbContext _context;
    private readonly ISolicitudesAuthorizationService _authz;

    public IncapacidadesController(IIncapacidadService service, ApplicationDbContext context, ISolicitudesAuthorizationService authz)
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

        var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
        if (!idEmpleadoActual.HasValue)
            return Forbid();

        return Ok(await _service.HistorialAsync(idEmpleadoActual.Value, fechaDesde, fechaHasta, idEstado));
    }

    [HttpPost]
    [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Crear([FromForm] IncapacidadCreateFormDTO form)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (form.Adjunto is { Length: > 0 })
        {
            var extension = Path.GetExtension(form.Adjunto.FileName).ToLowerInvariant();
            if (extension is not ".pdf" and not ".jpg" and not ".jpeg" and not ".png")
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["adjunto"] = ["Solo se permiten archivos PDF, JPG o PNG."]
                }));
            }
        }

        var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
        var dto = new IncapacidadCreateDTO
        {
            IdEmpleado = form.IdEmpleado,
            FechaInicio = form.FechaInicio,
            FechaFin = form.FechaFin,
            IdTipoIncapacidad = form.IdTipoIncapacidad,
            MontoCubierto = form.MontoCubierto,
            ComentarioSolicitud = form.ComentarioSolicitud
        };

        if (await _authz.EsAprobadorGlobalAsync(User))
        {
            // Puede crear para cualquier empleado.
        }
        else if (idEmpleadoActual.HasValue)
        {
            dto.IdEmpleado = idEmpleadoActual.Value;
        }
        else
        {
            return Forbid();
        }

        byte[]? archivoBytes = null;
        if (form.Adjunto is { Length: > 0 })
        {
            await using var ms = new MemoryStream();
            await form.Adjunto.CopyToAsync(ms);
            archivoBytes = ms.ToArray();
        }

        var creado = await _service.CrearAsync(dto, User.FindFirstValue(ClaimTypes.NameIdentifier), archivoBytes, form.Adjunto?.FileName);
        return CreatedAtAction(nameof(Historial), new { idEmpleado = creado.IdEmpleado }, creado);
    }

    [HttpPut("{id:int}")]
    [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Actualizar(int id, [FromForm] IncapacidadCreateFormDTO form)
    {
        if (id <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (form.Adjunto is { Length: > 0 })
        {
            var extension = Path.GetExtension(form.Adjunto.FileName).ToLowerInvariant();
            if (extension is not ".pdf" and not ".jpg" and not ".jpeg" and not ".png")
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["adjunto"] = ["Solo se permiten archivos PDF, JPG o PNG."]
                }));
            }
        }

        var incapacidad = await _context.Incapacidades.FirstOrDefaultAsync(x => x.IdIncapacidad == id);
        if (incapacidad is null)
            return NotFound("Incapacidad no encontrada.");
        if (!incapacidad.IdEmpleado.HasValue)
            return BadRequest("La incapacidad no tiene empleado asociado.");

        var esGlobal = await _authz.EsAprobadorGlobalAsync(User);
        var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
        if (!esGlobal && (!idEmpleadoActual.HasValue || idEmpleadoActual.Value != incapacidad.IdEmpleado.Value))
            return Forbid();

        var dto = new IncapacidadCreateDTO
        {
            IdEmpleado = incapacidad.IdEmpleado.Value,
            FechaInicio = form.FechaInicio,
            FechaFin = form.FechaFin,
            IdTipoIncapacidad = form.IdTipoIncapacidad,
            MontoCubierto = form.MontoCubierto,
            ComentarioSolicitud = form.ComentarioSolicitud
        };

        byte[]? archivoBytes = null;
        if (form.Adjunto is { Length: > 0 })
        {
            await using var ms = new MemoryStream();
            await form.Adjunto.CopyToAsync(ms);
            archivoBytes = ms.ToArray();
        }

        var actualizado = await _service.ActualizarRegistradaAsync(id, dto, User.FindFirstValue(ClaimTypes.NameIdentifier), archivoBytes, form.Adjunto?.FileName);
        return Ok(actualizado);
    }

    [HttpGet("tipos-disponibles")]
    [Obsolete("Deprecated: usar GET api/TipoIncapacidad para catálogos de combo.")]
    public async Task<IActionResult> TiposDisponibles()
    {
        var tipos = await _context.TipoIncapacidades
            .AsNoTracking()
            .Select(t => new
            {
                t.IdTipoIncapacidad,
                Nombre = EF.Property<string?>(t, nameof(TipoIncapacidad.Nombre)),
                IdEstado = EF.Property<int?>(t, nameof(TipoIncapacidad.IdEstado))
            })
            .Where(t =>
                !string.IsNullOrWhiteSpace(t.Nombre) &&
                t.IdEstado.HasValue)
            .OrderBy(t => t.Nombre)
            .ToListAsync();

        return Ok(tipos.Select(t => new TipoIncapacidad
        {
            IdTipoIncapacidad = t.IdTipoIncapacidad,
            Nombre = t.Nombre!,
            IdEstado = t.IdEstado!.Value
        }));
    }

    [HttpGet("estados-disponibles")]
    public async Task<IActionResult> EstadosDisponibles()
    {
        var estados = await _context.Estados
            .AsNoTracking()
            .Where(e => e.EstadoActivo == true || e.Nombre == "Registrada" || e.Nombre == "Validada" || e.Nombre == "Rechazada")
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        return Ok(estados);
    }

    [HttpGet("{id:int}/adjunto")]
    public async Task<IActionResult> DescargarAdjunto(int id)
    {
        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id inválido."] }));

        var registro = await _context.Incapacidades
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdIncapacidad == id);
        if (registro is null) return NotFound("Incapacidad no encontrada.");

        if (!await _authz.EsAprobadorGlobalAsync(User))
        {
            var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
            if (!idEmpleadoActual.HasValue || registro.IdEmpleado != idEmpleadoActual.Value)
                return Forbid();
        }

        var archivo = await _service.ObtenerAdjuntoAsync(id);
        if (archivo is null) return NotFound("No hay adjunto para esta incapacidad.");

        return File(archivo.Value.contenido, archivo.Value.contentType, archivo.Value.nombreArchivo);
    }

    [HttpPatch("{id:int}/validar")]
    public async Task<IActionResult> Validar(int id, [FromBody] SolicitudDecisionDTO? dto)
    {
        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id inválido."] }));
        if (!await _authz.EsAprobadorGlobalAsync(User))
            return Forbid();

        var actualizado = await _service.ValidarAsync(id, dto?.Comentario, User.FindFirstValue(ClaimTypes.NameIdentifier));
        return Ok(actualizado);
    }

    [HttpPatch("{id:int}/rechazar")]
    public async Task<IActionResult> Rechazar(int id, [FromBody] SolicitudDecisionDTO dto)
    {
        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id inválido."] }));
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (string.IsNullOrWhiteSpace(dto.Comentario))
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["comentario"] = ["El motivo de rechazo es obligatorio."] }));
        if (!await _authz.EsAprobadorGlobalAsync(User))
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

        var incapacidad = await _context.Incapacidades.FirstOrDefaultAsync(x => x.IdIncapacidad == id);
        if (incapacidad is null) return NotFound("Incapacidad no encontrada.");
        if (!incapacidad.IdEmpleado.HasValue) return BadRequest("La incapacidad no tiene empleado asociado.");

        var esEditar = string.Equals(dto.Accion, WorkflowAcciones.Editar, StringComparison.OrdinalIgnoreCase);
        if (esEditar)
        {
            var esGlobal = await _authz.EsAprobadorGlobalAsync(User);
            var idEmpleadoActual = await _authz.ObtenerIdEmpleadoActualAsync(User);
            if (!esGlobal && (!idEmpleadoActual.HasValue || idEmpleadoActual.Value != incapacidad.IdEmpleado.Value))
                return Forbid();
        }
        else if (!await _authz.EsAprobadorGlobalAsync(User))
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

public class IncapacidadCreateFormDTO
{
    [Range(1, int.MaxValue)]
    public int IdEmpleado { get; set; }

    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }

    [Range(1, int.MaxValue)]
    public int IdTipoIncapacidad { get; set; }

    public decimal? MontoCubierto { get; set; }

    [StringLength(300)]
    public string? ComentarioSolicitud { get; set; }

    public IFormFile? Adjunto { get; set; }
}
