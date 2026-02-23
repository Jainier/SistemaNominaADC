using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IAsistenciaService
{
    Task<List<Asistencia>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
    Task<Asistencia> RegistrarEntrada(AsistenciaMarcaDTO dto);
    Task<Asistencia> RegistrarSalida(AsistenciaMarcaDTO dto);
}
