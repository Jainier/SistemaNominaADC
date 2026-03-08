namespace SistemaNominaADC.Entidades.DTOs;

public class NominaConceptoDetalleDTO
{
    public DateTime? Fecha { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string GestionadoPor { get; set; } = "N/D";
    public decimal? Cantidad { get; set; }
    public string? UnidadCantidad { get; set; }
    public decimal? FactorAplicado { get; set; }
    public decimal Monto { get; set; }
}
