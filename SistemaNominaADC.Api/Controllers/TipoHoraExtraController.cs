using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TipoHoraExtraController : ControllerBase
{
    private readonly ITipoHoraExtraService _service;
    public TipoHoraExtraController(ITipoHoraExtraService service) => _service = service;
    [HttpGet] public async Task<IActionResult> Lista() => Ok(await _service.Lista());
    [HttpGet("{id:int}")] public async Task<IActionResult> Obtener(int id) => Ok(await _service.Obtener(id));
    [HttpPost] public async Task<IActionResult> Crear([FromBody] TipoHoraExtra dto) => Ok(await _service.Crear(dto));
    [HttpPut("{id:int}")] public async Task<IActionResult> Actualizar(int id, [FromBody] TipoHoraExtra dto) { if (id != dto.IdTipoHoraExtra) return BadRequest(); await _service.Actualizar(dto); return NoContent(); }
    [HttpDelete("Desactivar/{id:int}")] public async Task<IActionResult> Desactivar(int id) { await _service.Desactivar(id); return NoContent(); }
}
