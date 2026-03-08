using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IModoCalculoConceptoNominaService
{
    Task<List<ModoCalculoConceptoNomina>> Lista();
    Task<ModoCalculoConceptoNomina> Obtener(int id);
    Task<ModoCalculoConceptoNomina> Crear(ModoCalculoConceptoNomina modelo);
    Task<bool> Actualizar(ModoCalculoConceptoNomina modelo);
    Task<bool> Desactivar(int id);
}
