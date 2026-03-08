using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces
{
    public interface IGrupoEstadoService
    {
        Task<List<GrupoEstado>> Lista();
        Task<bool> Guardar(GrupoEstado entidad, List<int>? idsEstados = null);
        Task<bool> Eliminar(int id);
        Task<GrupoEstado?> ObtenerPorId(int id);
        Task<List<int>> ObtenerIdsEstadosAsociados(int idGrupoEstado);
    }
}
