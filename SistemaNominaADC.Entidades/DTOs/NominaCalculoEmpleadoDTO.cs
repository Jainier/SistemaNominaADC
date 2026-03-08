namespace SistemaNominaADC.Entidades.DTOs;

public class NominaCalculoEmpleadoDTO
{
    public int IdEmpleado { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public decimal SalarioBase { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal SalarioBruto { get; set; }
    public decimal TotalDeducciones { get; set; }
    public decimal SalarioNeto { get; set; }
    public List<NominaConceptoAplicadoDTO> Conceptos { get; set; } = [];
}
