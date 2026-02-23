using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaNominaADC.Entidades
{
    [Table("ObjetoSistemaRol")]
    public class ObjetoSistemaRol
    {
        public int IdObjeto { get; set; }
        public string RoleName { get; set; } = string.Empty;

        public ObjetoSistema? ObjetoSistema { get; set; }
    }
}
