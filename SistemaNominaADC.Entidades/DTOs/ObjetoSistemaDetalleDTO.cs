using System.Collections.Generic;

namespace SistemaNominaADC.Entidades.DTOs
{
    public class ObjetoSistemaDetalleDTO
    {
        public int IdObjeto { get; set; }
        public string NombreEntidad { get; set; } = string.Empty;
        public int IdGrupoEstado { get; set; }
        public string? NombreGrupoEstado { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
