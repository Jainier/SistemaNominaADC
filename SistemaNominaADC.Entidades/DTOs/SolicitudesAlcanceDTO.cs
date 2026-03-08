namespace SistemaNominaADC.Entidades.DTOs;

public class SolicitudesAlcanceDTO
{
    public bool EsAprobador { get; set; }
    public bool EsGlobal { get; set; }
    public int? IdEmpleadoActual { get; set; }
    public List<int> DepartamentosGestionados { get; set; } = new();
}
