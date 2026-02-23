using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaNominaADC.Entidades
{
    [Table("ObjetoSistema")]
    public class ObjetoSistema
    {
        [Key]
        public int IdObjeto { get; set; }

        [Required(ErrorMessage = "El nombre de la entidad es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        public string NombreEntidad { get; set; } = null!; 

        [Range(1, int.MaxValue, ErrorMessage = "Debe asignar un grupo de estados")]
        public int IdGrupoEstado { get; set; }

        [ForeignKey("IdGrupoEstado")]
        public virtual GrupoEstado? GrupoEstado { get; set; }
    }
}
