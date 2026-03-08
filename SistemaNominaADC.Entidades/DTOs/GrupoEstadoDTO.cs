using System.ComponentModel.DataAnnotations;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Entidades.DTO
{
    public class GrupoEstadoDTO
    {
        public int IdGrupoEstado { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        [RegularExpression(ValidacionPatrones.NombreGeneral, ErrorMessage = "El nombre solo puede contener letras y separadores válidos.")]
        public string? Nombre { get; set; }

        [StringLength(250, ErrorMessage = "La descripción no debe exceder 250 caracteres.")]
        public string? Descripcion { get; set; }
    }
}

