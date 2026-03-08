using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SolicitudesController : ControllerBase
{
    private readonly ISolicitudesAuthorizationService _authz;
    private readonly ApplicationDbContext _context;

    public SolicitudesController(ISolicitudesAuthorizationService authz, ApplicationDbContext context)
    {
        _authz = authz;
        _context = context;
    }

    [HttpGet("alcance")]
    public async Task<IActionResult> Alcance()
    {
        var esGlobal = await _authz.EsAprobadorGlobalAsync(User);
        var departamentos = await _authz.ObtenerDepartamentosGestionadosAsync(User);
        var idEmpleado = await _authz.ObtenerIdEmpleadoActualAsync(User);

        return Ok(new SolicitudesAlcanceDTO
        {
            EsGlobal = esGlobal,
            EsAprobador = esGlobal || departamentos.Count > 0,
            IdEmpleadoActual = idEmpleado,
            DepartamentosGestionados = departamentos
        });
    }

    [HttpGet("empleados-gestionables")]
    public async Task<IActionResult> EmpleadosGestionables()
    {
        var esGlobal = await _authz.EsAprobadorGlobalAsync(User);
        var empleadosGestionables = await _authz.ObtenerEmpleadosGestionablesAsync(User);

        if (!esGlobal && empleadosGestionables.Count == 0)
            return Forbid();

        var query = _context.Empleados
            .Include(e => e.Puesto)
            .AsQueryable();

        if (!esGlobal)
        {
            query = query.Where(e => empleadosGestionables.Contains(e.IdEmpleado));
        }

        var lista = await query
            .OrderBy(e => e.NombreCompleto)
            .Select(e => new
            {
                e.IdEmpleado,
                e.Cedula,
                e.NombreCompleto
            })
            .ToListAsync();

        return Ok(lista);
    }
}
