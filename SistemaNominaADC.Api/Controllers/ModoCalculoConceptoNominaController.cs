using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ModoCalculoConceptoNominaController : ControllerBase
{
    private readonly IModoCalculoConceptoNominaService _service;
    private readonly IObjetoSistemaAuthorizationService _objetoAuthService;

    public ModoCalculoConceptoNominaController(
        IModoCalculoConceptoNominaService service,
        IObjetoSistemaAuthorizationService objetoAuthService)
    {
        _service = service;
        _objetoAuthService = objetoAuthService;
    }

    [HttpGet]
    public async Task<IActionResult> Lista()
    {
        var acceso = await ValidarConsultaCatalogoAsync();
        if (acceso != null) return acceso;

        return Ok(await _service.Lista());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var acceso = await ValidarConsultaCatalogoAsync();
        if (acceso != null) return acceso;

        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido"] }));
        return Ok(await _service.Obtener(id));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ModoCalculoConceptoNomina dto)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var creado = await _service.Crear(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdModoCalculoConceptoNomina }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ModoCalculoConceptoNomina dto)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (id != dto.IdModoCalculoConceptoNomina) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["El id no coincide con el cuerpo"] }));
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

    private async Task<IActionResult?> ValidarAccesoModuloAsync()
    {
        var autorizado = await _objetoAuthService.PuedeAccederModuloAsync(User, "ModoCalculoConceptoNomina");
        return autorizado ? null : Forbid();
    }

    private async Task<IActionResult?> ValidarConsultaCatalogoAsync()
    {
        var autorizado = await _objetoAuthService.PuedeConsultarCatalogoAsync(User, "ModoCalculoConceptoNomina");
        return autorizado ? null : Forbid();
    }
}
