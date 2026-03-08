using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IDepartamentoJefaturaService
{
    Task<List<DepartamentoJefatura>> ListaAsync(int? idDepartamento = null, bool soloActivos = true);
    Task<DepartamentoJefatura> ObtenerAsync(int idDepartamentoJefatura);
    Task<DepartamentoJefatura> CrearAsync(DepartamentoJefatura entidad);
    Task<DepartamentoJefatura> ActualizarAsync(DepartamentoJefatura entidad);
    Task DesactivarAsync(int idDepartamentoJefatura);
    Task<List<int>> ObtenerDepartamentosGestionadosPorUserIdAsync(string identityUserId);
}
