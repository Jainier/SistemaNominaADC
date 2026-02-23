using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ObjetosSistemaController : ControllerBase
    {
        private readonly IObjetoSistemaService _objetoService;
        public ObjetosSistemaController(IObjetoSistemaService objetoService) => _objetoService = objetoService;

        [HttpGet("Lista")]
        [Authorize(Roles = "Admin,Administrador,ADMINISTRADOR")]
        public async Task<IActionResult> Lista() => Ok(await _objetoService.Lista());

        [HttpPost("Guardar")]
        [Authorize(Roles = "Admin,Administrador,ADMINISTRADOR")]
        public async Task<IActionResult> Guardar([FromBody] ObjetoSistemaCreateUpdateDTO entidad)
        {
            if (entidad == null)
                return BadRequest("La información del objeto es obligatoria.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(await _objetoService.Guardar(entidad));
        }

        [HttpGet("Obtener/{nombre}")]
        [Authorize(Roles = "Admin,Administrador,ADMINISTRADOR")]
        public async Task<IActionResult> Obtener(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("El nombre de la entidad es requerido.");

            return Ok(await _objetoService.ObtenerPorNombre(nombre));
        }

        [HttpDelete("Inactivar/{id:int}")]
        [Authorize(Roles = "Admin,Administrador,ADMINISTRADOR")]
        public async Task<IActionResult> Inactivar(int id)
        {
            if (id <= 0)
                return BadRequest("El id es inválido.");

            await _objetoService.Inactivar(id);
            return NoContent();
        }

        [HttpGet("ListaParaMenu")]
        public async Task<IActionResult> ListaParaMenu()
        {
            var roles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value);

            return Ok(await _objetoService.ListaParaMenu(roles));
        }
    }
}
