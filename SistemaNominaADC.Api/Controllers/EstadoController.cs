using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Negocio.Interfaces;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoController : ControllerBase
    {
        private readonly IEstadoService _estadoService;
        public EstadoController(IEstadoService estadoService) => _estadoService = estadoService;

        [HttpGet("Lista")]
        public async Task<IActionResult> Lista() => Ok(await _estadoService.Lista());

        [HttpGet("Obtener/{id}")]
        public async Task<IActionResult> Obtener(int id) => Ok(await _estadoService.Obtener(id));

        [HttpPost("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] EstadoRequest request) =>
            Ok(await _estadoService.Guardar(request.Entidad, request.IdsGrupos));

        [HttpGet("GruposAsociados/{id}")]
        public async Task<IActionResult> ObtenerIdsGruposAsociados(int id) =>
            Ok(await _estadoService.ObtenerIdsGruposAsociados(id));

        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id) => Ok(await _estadoService.Eliminar(id));

        [HttpGet("PorEntidad/{nombre}")]
        public async Task<IActionResult> ListarEstadosPorEntidad(string nombre) =>
            Ok(await _estadoService.ListarEstadosPorEntidad(nombre));
    }

    public class EstadoRequest
    {
        public Estado Entidad { get; set; } = null!;
        public List<int> IdsGrupos { get; set; } = new();
    }
}