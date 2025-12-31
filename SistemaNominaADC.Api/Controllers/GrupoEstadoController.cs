using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Negocio.Interfaces;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrupoEstadoController : ControllerBase
    {
        private readonly IGrupoEstadoService _grupoService;
        public GrupoEstadoController(IGrupoEstadoService grupoService) => _grupoService = grupoService;

        [HttpGet("Lista")]
        public async Task<IActionResult> Lista() => Ok(await _grupoService.Lista());

        [HttpPost("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] GrupoEstado entidad) => Ok(await _grupoService.Guardar(entidad));

        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id) => Ok(await _grupoService.Eliminar(id));

        [HttpGet("Obtener/{id}")]
        public async Task<IActionResult> Obtener(int id) => Ok(await _grupoService.ObtenerPorId(id));
    }
}
