using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades.DTO
{
    public class ObjetoSistemaDTO
    {
        public int IdObjeto { get; set; }
        public string NombreEntidad { get; set; } = null!;
        public int IdGrupoEstado { get; set; }
        public string? NombreGrupoEstado { get; set; }
    }
}
