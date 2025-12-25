using SistemaNominaADC.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Negocio.Interfaces
{
    public interface IObjetoSistemaService
    {
        Task<List<ObjetoSistema>> Lista();
        Task<bool> Guardar(ObjetoSistema entidad);
        Task<ObjetoSistema?> ObtenerPorNombre(string nombreEntidad);
    }
}
