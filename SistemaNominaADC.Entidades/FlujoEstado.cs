using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class FlujoEstado
{
    public int IdFlujoEstado { get; set; }

    [Required(ErrorMessage = "La entidad es obligatoria.")]
    [StringLength(100, ErrorMessage = "La entidad no debe exceder 100 caracteres.")]
    [RegularExpression(ValidacionPatrones.IdentificadorTecnico, ErrorMessage = "La entidad solo puede contener letras, números y guion bajo, iniciando con letra.")]
    public string Entidad { get; set; } = string.Empty;

    public int? IdEstadoOrigen { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El estado destino es obligatorio.")]
    public int IdEstadoDestino { get; set; }

    [Required(ErrorMessage = "La accion es obligatoria.")]
    [StringLength(50, ErrorMessage = "La accion no debe exceder 50 caracteres.")]
    [RegularExpression(ValidacionPatrones.IdentificadorTecnico, ErrorMessage = "La acción solo puede contener letras, números y guion bajo, iniciando con letra.")]
    public string Accion { get; set; } = string.Empty;

    [StringLength(256, ErrorMessage = "El rol requerido no debe exceder 256 caracteres.")]
    public string? RequiereRol { get; set; }

    public bool Activo { get; set; } = true;

    public Estado? EstadoOrigen { get; set; }
    public Estado? EstadoDestino { get; set; }
}
