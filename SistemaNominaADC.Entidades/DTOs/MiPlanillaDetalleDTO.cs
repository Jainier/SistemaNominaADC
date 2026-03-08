namespace SistemaNominaADC.Entidades.DTOs;

public class MiPlanillaDetalleDTO
{
    public int IdPlanilla { get; set; }
    public int IdEmpleado { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public decimal SalarioBaseMensual { get; set; }
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFin { get; set; }
    public DateTime FechaPago { get; set; }
    public string TipoPlanilla { get; set; } = string.Empty;
    public string EstadoPlanilla { get; set; } = string.Empty;
    public NominaCalculoEmpleadoDTO Detalle { get; set; } = new();
}
