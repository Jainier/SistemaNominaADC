using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades
{
    public class GrupoEstado
    {
        [Key]
        public int IdGrupoEstado { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        public string? Nombre { get; set; } 
        [StringLength(250, ErrorMessage = "La descripción no debe exceder 250 caracteres.")]
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
    }
}
