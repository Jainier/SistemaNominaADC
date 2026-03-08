using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EmpleadoJerarquiaController : ControllerBase
{
    private readonly IEmpleadoJerarquiaService _service;
    private readonly IObjetoSistemaAuthorizationService _objetoAuthService;

    public EmpleadoJerarquiaController(
        IEmpleadoJerarquiaService service,
        IObjetoSistemaAuthorizationService objetoAuthService)
    {
        _service = service;
        _objetoAuthService = objetoAuthService;
    }

    [HttpGet]
    public async Task<IActionResult> Lista([FromQuery] int? idSupervisor, [FromQuery] int? idEmpleado, [FromQuery] bool soloActivos = true)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        return Ok(await _service.ListaAsync(idSupervisor, idEmpleado, soloActivos));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (id <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));

        return Ok(await _service.ObtenerAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] EmpleadoJerarquia dto)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var creado = await _service.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdEmpleadoJerarquia }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] EmpleadoJerarquia dto)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (id != dto.IdEmpleadoJerarquia)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["El id no coincide con el cuerpo."] }));

        var actualizado = await _service.ActualizarAsync(dto);
        return Ok(actualizado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (id <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido."] }));

        await _service.DesactivarAsync(id);
        return NoContent();
    }

    private async Task<IActionResult?> ValidarAccesoModuloAsync()
    {
        var autorizado = await _objetoAuthService.PuedeAccederModuloAsync(User, "EmpleadoJerarquia");
        return autorizado ? null : Forbid();
    }
}
