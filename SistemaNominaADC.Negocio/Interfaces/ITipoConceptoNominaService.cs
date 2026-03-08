using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface ITipoConceptoNominaService
{
    Task<List<TipoConceptoNomina>> Lista();
    Task<TipoConceptoNomina> Obtener(int id);
    Task<TipoConceptoNomina> Crear(TipoConceptoNomina modelo);
    Task<bool> Actualizar(TipoConceptoNomina modelo);
    Task<bool> Desactivar(int id);
}
