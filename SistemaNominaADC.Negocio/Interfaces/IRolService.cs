using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Negocio.Interfaces
{
    public interface IRolService
    {
        Task<IEnumerable<RolDTO>> ObtenerTodosAsync();
        Task<RolDTO> ObtenerPorIdAsync(string sRolId);
        Task<string> CrearAsync(RolCreateUpdateDTO dto);
        Task ActualizarAsync(string sRolId, RolCreateUpdateDTO dto);
        Task EliminarAsync(string sRolId);
    }
}
