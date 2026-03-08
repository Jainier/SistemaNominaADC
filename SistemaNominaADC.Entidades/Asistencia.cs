using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class Asistencia
{
    public int IdAsistencia { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today;

    public DateTime? HoraEntrada { get; set; }
    public DateTime? HoraSalida { get; set; }
    public bool? Ausencia { get; set; }
    [StringLength(1000, ErrorMessage = "La justificación no debe exceder 1000 caracteres.")]
    public string? Justificacion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int IdEstado { get; set; }

    public Empleado? Empleado { get; set; }
    public Estado? Estado { get; set; }
}
