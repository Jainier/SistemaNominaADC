namespace SistemaNominaADC.Entidades.DTOs;

public class MiPlanillaHistorialItemDTO
{
    public int IdPlanilla { get; set; }
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFin { get; set; }
    public DateTime FechaPago { get; set; }
    public string TipoPlanilla { get; set; } = string.Empty;
    public string EstadoPlanilla { get; set; } = string.Empty;
    public decimal SalarioBase { get; set; }
    public decimal SalarioBruto { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalDeducciones { get; set; }
    public decimal SalarioNeto { get; set; }
}

