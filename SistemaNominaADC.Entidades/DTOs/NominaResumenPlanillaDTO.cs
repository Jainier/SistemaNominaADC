namespace SistemaNominaADC.Entidades.DTOs;

public class NominaResumenPlanillaDTO
{
    public int IdPlanilla { get; set; }
    public string Accion { get; set; } = string.Empty;
    public DateTime FechaProceso { get; set; }
    public int EmpleadosProcesados { get; set; }
    public decimal TotalBrutoPlanilla { get; set; }
    public decimal TotalDeduccionesPlanilla { get; set; }
    public decimal TotalNetoPlanilla { get; set; }
    public List<NominaCalculoEmpleadoDTO> Empleados { get; set; } = [];
}
