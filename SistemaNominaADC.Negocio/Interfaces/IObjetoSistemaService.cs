using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Negocio.Interfaces
{
    public interface IObjetoSistemaService
    {
        Task<List<ObjetoSistemaDetalleDTO>> Lista();
        Task<bool> Guardar(ObjetoSistemaCreateUpdateDTO entidad);
        Task<bool> Inactivar(int idObjeto);
        Task<ObjetoSistemaDetalleDTO?> ObtenerPorNombre(string nombreEntidad);
        Task<List<ObjetoSistemaDetalleDTO>> ListaParaMenu(IEnumerable<string> rolesUsuario);
    }
}
