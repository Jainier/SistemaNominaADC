using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface ITipoPlanillaConceptoService
{
    Task<List<TipoPlanillaConcepto>> Lista(int? idTipoPlanilla = null);
    Task<TipoPlanillaConcepto> Obtener(int idTipoPlanilla, int idConceptoNomina);
    Task<TipoPlanillaConcepto> Crear(TipoPlanillaConcepto modelo);
    Task<bool> Actualizar(TipoPlanillaConcepto modelo);
    Task<bool> Desactivar(int idTipoPlanilla, int idConceptoNomina);
}
