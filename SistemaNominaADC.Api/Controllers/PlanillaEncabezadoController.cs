using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Interfaces;
using SistemaNominaADC.Negocio.Servicios;
using System.Security.Claims;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PlanillaEncabezadoController : ControllerBase
{
    private readonly IPlanillaEncabezadoService _service;
    private readonly INominaService _nominaService;
    private readonly IComprobantePlanillaService _comprobantePlanillaService;
    private readonly IObjetoSistemaAuthorizationService _objetoAuthService;

    public PlanillaEncabezadoController(
        IPlanillaEncabezadoService service,
        INominaService nominaService,
        IComprobantePlanillaService comprobantePlanillaService,
        IObjetoSistemaAuthorizationService objetoAuthService)
    {
        _service = service;
        _nominaService = nominaService;
        _comprobantePlanillaService = comprobantePlanillaService;
        _objetoAuthService = objetoAuthService;
    }

    [HttpGet]
    public async Task<IActionResult> Lista()
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        return Ok(await _service.Lista());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido"] }));
        return Ok(await _service.Obtener(id));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] PlanillaEncabezado dto)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var creado = await _service.Crear(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdPlanilla }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] PlanillaEncabezado dto)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (id != dto.IdPlanilla) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["El id no coincide con el cuerpo"] }));
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        await _service.Actualizar(dto);
        return NoContent();
    }

    [HttpDelete("Desactivar/{id:int}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido"] }));
        await _service.Desactivar(id);
        return NoContent();
    }

    [HttpGet("{id:int}/acciones")]
    public async Task<IActionResult> AccionesDisponibles(int id)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;
        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido"] }));

        var acciones = await _service.ObtenerAccionesDisponibles(id, ObtenerRolesUsuario());
        return Ok(acciones);
    }

    [HttpPost("{id:int}/accion")]
    public async Task<IActionResult> EjecutarAccion(int id, [FromBody] EjecutarAccionWorkflowDTO dto)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;
        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido"] }));
        if (dto is null || string.IsNullOrWhiteSpace(dto.Accion))
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["accion"] = ["La accion es obligatoria."] }));

        var accion = dto.Accion.Trim();
        if (string.Equals(accion, WorkflowAcciones.Calcular, StringComparison.OrdinalIgnoreCase))
        {
            var resumen = await _nominaService.CalcularPlanilla(id, User.FindFirstValue(ClaimTypes.NameIdentifier), ObtenerRolesUsuario());
            return Ok(resumen);
        }

        if (string.Equals(accion, WorkflowAcciones.Recalcular, StringComparison.OrdinalIgnoreCase))
        {
            var resumen = await _nominaService.RecalcularPlanilla(id, User.FindFirstValue(ClaimTypes.NameIdentifier), ObtenerRolesUsuario());
            return Ok(resumen);
        }

        if (string.Equals(accion, WorkflowAcciones.Aprobar, StringComparison.OrdinalIgnoreCase))
        {
            await _nominaService.AprobarPlanilla(id, User.FindFirstValue(ClaimTypes.NameIdentifier), ObtenerRolesUsuario());
            return NoContent();
        }

        if (string.Equals(accion, WorkflowAcciones.Rechazar, StringComparison.OrdinalIgnoreCase))
        {
            await _nominaService.RechazarPlanilla(id, User.FindFirstValue(ClaimTypes.NameIdentifier), ObtenerRolesUsuario());
            return NoContent();
        }

        await _service.EjecutarAccionAsync(id, accion, ObtenerRolesUsuario());
        return NoContent();
    }

    [HttpGet("{id:int}/resumen")]
    public async Task<IActionResult> Resumen(int id)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;
        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido"] }));

        var resultado = await _nominaService.ObtenerResumenPlanilla(id);
        return Ok(resultado);
    }

    [HttpGet("{id:int}/comprobantes-zip")]
    public async Task<IActionResult> DescargarComprobantesZip(int id)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;
        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido"] }));

        var roles = ObtenerRolesUsuario();
        var acciones = await _service.ObtenerAccionesDisponibles(id, roles);
        if (!acciones.Any(EsAccionDescargarColillas))
            return Forbid();

        var (contenidoZip, nombreArchivoZip) = await _comprobantePlanillaService.GenerarZipComprobantesPlanillaAsync(id);
        return File(contenidoZip, "application/zip", nombreArchivoZip);
    }

    private async Task<IActionResult?> ValidarAccesoModuloAsync()
    {
        var autorizado = await _objetoAuthService.PuedeAccederModuloAsync(User, "PlanillaEncabezado");
        return autorizado ? null : Forbid();
    }

    private List<string> ObtenerRolesUsuario() =>
        User.Claims
            .Where(c => c.Type == ClaimTypes.Role && !string.IsNullOrWhiteSpace(c.Value))
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static bool EsAccionDescargarColillas(string? accion)
    {
        if (string.IsNullOrWhiteSpace(accion))
            return false;

        var normalizada = accion.Trim().Replace(" ", string.Empty).Replace("_", string.Empty);
        return string.Equals(normalizada, "DESCARGARCOLILLAS", StringComparison.OrdinalIgnoreCase);
    }
}
