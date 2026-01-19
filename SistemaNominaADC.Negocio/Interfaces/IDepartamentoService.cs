using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTO;

namespace SistemaNominaADC.Negocio.Interfaces
{
    public interface IDepartamentoService
    {
        Task<List<Departamento>> Lista();
        Task<Departamento?> Obtener(int id);
        Task<bool> Crear(DepartamentoDTO modelo);
        Task<bool> Actualizar(Departamento modelo);
        Task<bool> Eliminar(int id);
    }
}