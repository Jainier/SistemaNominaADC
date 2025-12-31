using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Negocio.Interfaces;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartamentoController : ControllerBase
    {
        private readonly IDepartamentoService _departamentoService;
        public DepartamentoController(IDepartamentoService departamentoService) => _departamentoService = departamentoService;

        [HttpGet("Lista")]
        public async Task<IActionResult> Lista() => Ok(await _departamentoService.Lista());

        [HttpGet("Obtener/{id}")]
        public async Task<IActionResult> Obtener(int id) => Ok(await _departamentoService.Obtener(id));

        [HttpPost("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] Departamento modelo) => Ok(await _departamentoService.Guardar(modelo));

        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id) => Ok(await _departamentoService.Eliminar(id));
    }
}