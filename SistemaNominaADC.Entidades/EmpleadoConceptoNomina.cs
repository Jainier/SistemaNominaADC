using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class EmpleadoConceptoNomina
{
    public int IdEmpleadoConceptoNomina { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El concepto de nomina es obligatorio.")]
    public int IdConceptoNomina { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto fijo no puede ser negativo.")]
    public decimal? MontoFijo { get; set; }

    [Range(0, 1, ErrorMessage = "El porcentaje debe estar entre 0 y 1.")]
    public decimal? Porcentaje { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El saldo pendiente no puede ser negativo.")]
    public decimal? SaldoPendiente { get; set; }

    public int Prioridad { get; set; }
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public bool Activo { get; set; } = true;

    public Empleado? Empleado { get; set; }
    public TipoConceptoNomina? TipoConceptoNomina { get; set; }
}
