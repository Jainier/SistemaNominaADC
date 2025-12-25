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
        public string NombreEntidad { get; set; } = null!; 

        [Required(ErrorMessage = "Debe asignar un grupo de estados")]
        public int IdGrupoEstado { get; set; }

        [ForeignKey("IdGrupoEstado")]
        public virtual GrupoEstado? GrupoEstado { get; set; }
    }
}