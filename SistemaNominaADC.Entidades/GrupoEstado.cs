using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades
{
    public class GrupoEstado
    {
        [Key]
        public int IdGrupoEstado { get; set; }
        public string? Nombre { get; set; } 
        public string? Descripcion { get; set; }
    }
}
