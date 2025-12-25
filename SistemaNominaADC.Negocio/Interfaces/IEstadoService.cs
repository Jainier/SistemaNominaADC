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
        Task<Estado> Obtener(int id);
        Task<bool> Guardar(Estado modelo);
        Task<bool> Eliminar(int id);
    }
}