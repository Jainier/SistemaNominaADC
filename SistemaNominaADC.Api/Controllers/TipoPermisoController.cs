using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TipoPermisoController : ControllerBase
{
    private readonly ITipoPermisoService _service;
    public TipoPermisoController(ITipoPermisoService service) => _service = service;
    [HttpGet] public async Task<IActionResult> Lista() => Ok(await _service.Lista());
    [HttpGet("{id:int}")] public async Task<IActionResult> Obtener(int id) => Ok(await _service.Obtener(id));
    [HttpPost] public async Task<IActionResult> Crear([FromBody] TipoPermiso dto) => Ok(await _service.Crear(dto));
    [HttpPut("{id:int}")] public async Task<IActionResult> Actualizar(int id, [FromBody] TipoPermiso dto) { if (id != dto.IdTipoPermiso) return BadRequest(); await _service.Actualizar(dto); return NoContent(); }
    [HttpDelete("Desactivar/{id:int}")] public async Task<IActionResult> Desactivar(int id) { await _service.Desactivar(id); return NoContent(); }
}
