using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class Vacaciones
{
    public int IdVacaciones { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int? IdEmpleado { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Los días restantes no pueden ser negativos.")]
    public int? DiasRestantes { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int? IdEstado { get; set; }

    public Empleado? Empleado { get; set; }
    public Estado? Estado { get; set; }
}
