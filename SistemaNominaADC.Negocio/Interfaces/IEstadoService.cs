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
        // Actualizamos esta línea para recibir los dos parámetros
        Task<bool> Guardar(Estado entidad, List<int> idsGrupos);
        Task<bool> Eliminar(int id);
        // Agregamos este para que la edición funcione
        Task<List<int>> ObtenerIdsGruposAsociados(int idEstado);

        Task<List<Estado?>> ListarEstadosPorEntidad(string nombreEntidad);
    }
}