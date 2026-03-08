using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface ITipoPlanillaService
{
    Task<List<TipoPlanilla>> Lista(bool soloActivos = false);
    Task<TipoPlanilla> Obtener(int id);
    Task<TipoPlanilla> Crear(TipoPlanilla modelo);
    Task<bool> Actualizar(TipoPlanilla modelo);
    Task<bool> Desactivar(int id);
}
