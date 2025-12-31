using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces
{
    public interface IEstadoService
    {
        Task<List<Estado>> Lista();
        Task<Estado?> Obtener(int id);
        Task<bool> Guardar(Estado entidad, List<int> idsGrupos);
        Task<bool> Eliminar(int id);
        Task<List<int>> ObtenerIdsGruposAsociados(int idEstado);

        Task<List<Estado?>> ListarEstadosPorEntidad(string nombreEntidad);
    }
}