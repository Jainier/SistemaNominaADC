using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Entidades.DTOs
{
    public class ObjetoSistemaCreateUpdateDTO
    {
        public int IdObjeto { get; set; }

        [Required(ErrorMessage = "El nombre de la entidad es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre de la entidad no debe exceder 100 caracteres.")]
        [RegularExpression(ValidacionPatrones.IdentificadorTecnico, ErrorMessage = "La entidad solo puede contener letras, números y guion bajo, iniciando con letra.")]
        public string NombreEntidad { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Debe asignar un grupo de estados")]
        public int IdGrupoEstado { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}
