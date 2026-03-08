namespace SistemaNominaADC.Presentacion.Components.Shared;

public class SolicitudDetalleItem
{
    public SolicitudDetalleItem()
    {
    }

    public SolicitudDetalleItem(string etiqueta, string? valor)
    {
        Etiqueta = etiqueta;
        Valor = valor;
    }

    public string Etiqueta { get; set; } = string.Empty;
    public string? Valor { get; set; }
}
