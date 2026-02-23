namespace SistemaNominaADC.Entidades.DTO;

public class TipoHoraExtraDTO
{
    public int IdTipoHoraExtra { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal? PorcentajePago { get; set; }
    public bool Estado { get; set; } = true;
}
