using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades.DTOs
{
    public class ObjetoSistemaCreateUpdateDTO
    {
        public int IdObjeto { get; set; }

        [Required(ErrorMessage = "El nombre de la entidad es obligatorio")]
        public string NombreEntidad { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Debe asignar un grupo de estados")]
        public int IdGrupoEstado { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}
