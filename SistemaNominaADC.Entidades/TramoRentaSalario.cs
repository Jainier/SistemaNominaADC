using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class TramoRentaSalario
{
    public int IdTramoRentaSalario { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto desde no puede ser negativo.")]
    public decimal DesdeMonto { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto hasta no puede ser negativo.")]
    public decimal? HastaMonto { get; set; }

    [Range(0, 1, ErrorMessage = "La tasa debe estar entre 0 y 1.")]
    public decimal Tasa { get; set; }

    public DateTime VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }

    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
}
