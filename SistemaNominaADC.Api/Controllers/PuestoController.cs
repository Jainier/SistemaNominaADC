using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PuestoController : ControllerBase
{
    private readonly IPuestoService _service;
    public PuestoController(IPuestoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Lista() => Ok(await _service.Lista());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        if (id <= 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["Id inválido"] }));
        return Ok(await _service.Obtener(id));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] Puesto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var creado = await _service.Crear(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdPuesto }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] Puesto dto)
    {
        if (id != dto.IdPuesto) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["id"] = ["El id no coincide con el cuerpo"] }));
        await _service.Actualizar(dto);
        return NoContent();
    }

    [HttpDelete("Desactivar/{id:int}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        await _service.Desactivar(id);
        return NoContent();
    }
}
