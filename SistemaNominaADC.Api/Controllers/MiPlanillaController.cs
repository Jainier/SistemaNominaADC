using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MiPlanillaController : ControllerBase
{
    private readonly IMiPlanillaService _miPlanillaService;
    private readonly ApplicationDbContext _context;
    private readonly ISolicitudesAuthorizationService _solicitudesAuth;
    private readonly IObjetoSistemaAuthorizationService _objetoAuthService;
    private readonly IComprobantePlanillaService _comprobantePlanillaService;

    public MiPlanillaController(
        IMiPlanillaService miPlanillaService,
        ApplicationDbContext context,
        ISolicitudesAuthorizationService solicitudesAuth,
        IObjetoSistemaAuthorizationService objetoAuthService,
        IComprobantePlanillaService comprobantePlanillaService)
    {
        _miPlanillaService = miPlanillaService;
        _context = context;
        _solicitudesAuth = solicitudesAuth;
        _objetoAuthService = objetoAuthService;
        _comprobantePlanillaService = comprobantePlanillaService;
    }

    [HttpGet("historial")]
    public async Task<IActionResult> Historial()
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        var idEmpleado = await _solicitudesAuth.ObtenerIdEmpleadoActualAsync(User);
        if (!idEmpleado.HasValue || idEmpleado.Value <= 0)
            return ForbidConDetalle("El usuario autenticado no esta vinculado a un empleado.");

        var historial = await _miPlanillaService.HistorialPorEmpleadoAsync(idEmpleado.Value);
        return Ok(historial);
    }

    [HttpGet("{idPlanilla:int}/detalle")]
    public async Task<IActionResult> Detalle(int idPlanilla)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;
        if (idPlanilla <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["idPlanilla"] = ["Id invalido."] }));

        var idEmpleado = await _solicitudesAuth.ObtenerIdEmpleadoActualAsync(User);
        if (!idEmpleado.HasValue || idEmpleado.Value <= 0)
            return ForbidConDetalle("El usuario autenticado no esta vinculado a un empleado.");

        var detalle = await _miPlanillaService.ObtenerDetallePorEmpleadoAsync(idEmpleado.Value, idPlanilla);
        return Ok(detalle);
    }

    [HttpGet("{idPlanilla:int}/pdf")]
    public async Task<IActionResult> Pdf(int idPlanilla)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;
        if (idPlanilla <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["idPlanilla"] = ["Id invalido."] }));

        var idEmpleado = await _solicitudesAuth.ObtenerIdEmpleadoActualAsync(User);
        if (!idEmpleado.HasValue || idEmpleado.Value <= 0)
            return ForbidConDetalle("El usuario autenticado no esta vinculado a un empleado.");

        var detalleDb = await _context.PlanillasDetalle
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdPlanilla == idPlanilla && x.IdEmpleado == idEmpleado.Value);

        if (detalleDb?.ComprobantePdf is { Length: > 0 })
        {
            var nombreGuardado = string.IsNullOrWhiteSpace(detalleDb.NombreComprobantePdf)
                ? $"Planilla_{idPlanilla}_Emp{idEmpleado.Value}.pdf"
                : detalleDb.NombreComprobantePdf;

            return File(detalleDb.ComprobantePdf, "application/pdf", nombreGuardado);
        }

        await _comprobantePlanillaService.GenerarYGuardarComprobantesPlanillaAsync(idPlanilla);

        var detalleActualizado = await _context.PlanillasDetalle
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdPlanilla == idPlanilla && x.IdEmpleado == idEmpleado.Value);

        if (detalleActualizado?.ComprobantePdf is not { Length: > 0 })
            return NotFound("No fue posible generar la colilla para esta planilla.");

        var nombreGenerado = string.IsNullOrWhiteSpace(detalleActualizado.NombreComprobantePdf)
            ? $"Planilla_{idPlanilla}_Emp{idEmpleado.Value}.pdf"
            : detalleActualizado.NombreComprobantePdf;

        return File(detalleActualizado.ComprobantePdf, "application/pdf", nombreGenerado);
    }

    private async Task<IActionResult?> ValidarAccesoModuloAsync()
    {
        var autorizado = await _objetoAuthService.PuedeAccederModuloAsync(User, "MiPlanilla");
        return autorizado ? null : ForbidConDetalle("No tienes permisos para acceder al modulo Mis Planillas.");
    }

    private ObjectResult ForbidConDetalle(string detalle)
        => StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
        {
            Title = "No autorizado",
            Status = StatusCodes.Status403Forbidden,
            Detail = detalle
        });
}

