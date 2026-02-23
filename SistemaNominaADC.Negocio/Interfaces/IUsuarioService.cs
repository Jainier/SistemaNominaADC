using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Negocio.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDTO>> ObtenerTodosAsync();
        Task<UsuarioDTO> ObtenerPorIdAsync(string id);
        Task<string> CrearAsync(UsuarioCreateDTO dto);
        Task ActualizarAsync(string id, UsuarioUpdateDTO dto);
        Task CambiarPasswordAsync(string id, UsuarioPasswordDTO dto);
        Task CambiarEstadoAsync(string id, bool activo);
    }
}
