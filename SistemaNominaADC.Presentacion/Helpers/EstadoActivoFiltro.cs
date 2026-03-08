using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Presentacion.Helpers;

public static class EstadoActivoFiltro
{
    public static HashSet<int> ObtenerIdsActivos(IEnumerable<Estado> estados) =>
        estados
            .Where(e => e.EstadoActivo != false)
            .Select(e => e.IdEstado)
            .ToHashSet();

    public static bool EstaActivo(int idEstado, IReadOnlySet<int> idsActivos) =>
        idsActivos.Count == 0 || idsActivos.Contains(idEstado);
}
