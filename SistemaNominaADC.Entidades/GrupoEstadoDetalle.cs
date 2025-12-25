using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaNominaADC.Entidades
{
    [Table("GrupoEstadoDetalle")]
    public class GrupoEstadoDetalle
    {
        public int IdGrupoEstado { get; set; }
        public int IdEstado { get; set; }
        public int? Orden { get; set; }

        [ForeignKey("IdGrupoEstado")]
        public virtual GrupoEstado? GrupoEstado { get; set; }

        [ForeignKey("IdEstado")]
        public virtual Estado? Estado { get; set; }
    }
}