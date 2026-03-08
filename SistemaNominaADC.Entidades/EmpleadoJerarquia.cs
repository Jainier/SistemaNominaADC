using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class EmpleadoJerarquia
{
    public int IdEmpleadoJerarquia { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El supervisor es obligatorio.")]
    public int IdSupervisor { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }

    [StringLength(250, ErrorMessage = "La observación no debe exceder 250 caracteres.")]
    public string? Observacion { get; set; }

    public Empleado? Empleado { get; set; }
    public Empleado? Supervisor { get; set; }
}
