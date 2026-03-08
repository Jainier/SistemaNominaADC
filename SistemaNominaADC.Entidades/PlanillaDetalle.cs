using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class PlanillaDetalle
{
    public int IdPlanillaDetalle { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La planilla es obligatoria.")]
    public int IdPlanilla { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El salario base no puede ser negativo.")]
    public decimal SalarioBase { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El total de ingresos no puede ser negativo.")]
    public decimal TotalIngresos { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El total de deducciones no puede ser negativo.")]
    public decimal TotalDeducciones { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El salario bruto no puede ser negativo.")]
    public decimal SalarioBruto { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El salario neto no puede ser negativo.")]
    public decimal SalarioNeto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int IdEstado { get; set; }

    [MaxLength(260)]
    public string? NombreComprobantePdf { get; set; }

    [MaxLength(64)]
    public string? HashComprobantePdf { get; set; }

    public byte[]? ComprobantePdf { get; set; }

    public DateTime? FechaGeneracionComprobantePdf { get; set; }

    public PlanillaEncabezado? Planilla { get; set; }
    public Empleado? Empleado { get; set; }
    public Estado? Estado { get; set; }
}
