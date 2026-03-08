namespace SistemaNominaADC.Entidades.DTOs;

public class NominaConceptoAplicadoDTO
{
    public int IdConceptoNomina { get; set; }
    public string CodigoConcepto { get; set; } = string.Empty;
    public string NombreConcepto { get; set; } = string.Empty;
    public decimal? PorcentajeAplicado { get; set; }
    public decimal Monto { get; set; }
    public bool EsIngreso { get; set; }
    public bool EsDeduccion { get; set; }
    public bool AfectaCcss { get; set; }
    public bool EsSalarioBruto { get; set; }
    public bool EsAjusteNoLaborado { get; set; }
    public decimal? Cantidad { get; set; }
    public string? UnidadCantidad { get; set; }
    public List<NominaConceptoDetalleDTO> Detalles { get; set; } = [];
}
