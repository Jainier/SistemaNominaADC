namespace SistemaNominaADC.Presentacion.Helpers;

public static class FiltroFechasHelper
{
    public static DateTime PrimerDiaMesActual()
        => new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public static DateTime UltimoDiaMesActual()
        => PrimerDiaMesActual().AddMonths(1).AddDays(-1);
}

