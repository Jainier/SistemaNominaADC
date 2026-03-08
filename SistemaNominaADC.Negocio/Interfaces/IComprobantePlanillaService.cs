namespace SistemaNominaADC.Negocio.Interfaces;

public interface IComprobantePlanillaService
{
    Task GenerarYGuardarComprobantesPlanillaAsync(int idPlanilla);
    Task<(byte[] contenidoZip, string nombreArchivoZip)> GenerarZipComprobantesPlanillaAsync(int idPlanilla);
}
