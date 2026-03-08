using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TipoPlanillaConceptoController : ControllerBase
{
    private readonly ITipoPlanillaConceptoService _service;
    private readonly IObjetoSistemaAuthorizationService _objetoAuthService;

    public TipoPlanillaConceptoController(
        ITipoPlanillaConceptoService service,
        IObjetoSistemaAuthorizationService objetoAuthService)
    {
        _service = service;
        _objetoAuthService = objetoAuthService;
    }

    [HttpGet]
    public async Task<IActionResult> Lista([FromQuery] int? idTipoPlanilla = null)
    {
        var acceso = await ValidarConsultaCatalogoAsync();
        if (acceso != null) return acceso;

        return Ok(await _service.Lista(idTipoPlanilla));
    }

    [HttpGet("{idTipoPlanilla:int}/{idConceptoNomina:int}")]
    public async Task<IActionResult> Obtener(int idTipoPlanilla, int idConceptoNomina)
    {
        var acceso = await ValidarConsultaCatalogoAsync();
        if (acceso != null) return acceso;

        if (idTipoPlanilla <= 0 || idConceptoNomina <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido"] }));

        return Ok(await _service.Obtener(idTipoPlanilla, idConceptoNomina));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] TipoPlanillaConcepto dto)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var creado = await _service.Crear(dto);
        return CreatedAtAction(nameof(Obtener), new { idTipoPlanilla = creado.IdTipoPlanilla, idConceptoNomina = creado.IdConceptoNomina }, creado);
    }

    [HttpPut("{idTipoPlanilla:int}/{idConceptoNomina:int}")]
    public async Task<IActionResult> Actualizar(int idTipoPlanilla, int idConceptoNomina, [FromBody] TipoPlanillaConcepto dto)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (idTipoPlanilla != dto.IdTipoPlanilla || idConceptoNomina != dto.IdConceptoNomina)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Los ids no coinciden con el cuerpo"] }));

        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        await _service.Actualizar(dto);
        return NoContent();
    }

    [HttpDelete("Desactivar/{idTipoPlanilla:int}/{idConceptoNomina:int}")]
    public async Task<IActionResult> Desactivar(int idTipoPlanilla, int idConceptoNomina)
    {
        var acceso = await ValidarAccesoModuloAsync();
        if (acceso != null) return acceso;

        if (idTipoPlanilla <= 0 || idConceptoNomina <= 0)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id invalido"] }));

        await _service.Desactivar(idTipoPlanilla, idConceptoNomina);
        return NoContent();
    }

    private async Task<IActionResult?> ValidarAccesoModuloAsync()
    {
        var autorizado = await _objetoAuthService.PuedeAccederModuloAsync(User, "TipoPlanillaConcepto");
        return autorizado ? null : Forbid();
    }

    private async Task<IActionResult?> ValidarConsultaCatalogoAsync()
    {
        var autorizado = await _objetoAuthService.PuedeConsultarCatalogoAsync(User, "TipoPlanillaConcepto");
        return autorizado ? null : Forbid();
    }
}
