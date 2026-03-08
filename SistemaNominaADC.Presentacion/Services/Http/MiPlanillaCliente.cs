using SistemaNominaADC.Entidades.DTOs;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IMiPlanillaCliente
{
    Task<List<MiPlanillaHistorialItemDTO>> Historial();
    Task<MiPlanillaDetalleDTO?> Detalle(int idPlanilla);
    Task<(byte[] contenido, string nombreArchivo, string contentType)?> DescargarPdf(int idPlanilla);
}

public class MiPlanillaCliente : IMiPlanillaCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public MiPlanillaCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<MiPlanillaHistorialItemDTO>> Historial()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/MiPlanilla/historial");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar historial de planilla.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<MiPlanillaHistorialItemDTO>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar historial de planilla: {ex.Message}");
            return new();
        }
    }

    public async Task<MiPlanillaDetalleDTO?> Detalle(int idPlanilla)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idPlanilla, "id de planilla")) return null;

        try
        {
            var response = await _http.GetAsync($"api/MiPlanilla/{idPlanilla}/detalle");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar detalle de planilla.");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MiPlanillaDetalleDTO>();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar detalle de planilla: {ex.Message}");
            return null;
        }
    }

    public async Task<(byte[] contenido, string nombreArchivo, string contentType)?> DescargarPdf(int idPlanilla)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idPlanilla, "id de planilla"))
            return null;

        try
        {
            var response = await _http.GetAsync($"api/MiPlanilla/{idPlanilla}/pdf");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para descargar el PDF de planilla.");
                return null;
            }

            var contenido = await response.Content.ReadAsByteArrayAsync();
            var nombreArchivo = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName
                ?? $"planilla_{idPlanilla}.pdf";
            nombreArchivo = nombreArchivo.Trim('"');

            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = "application/pdf";

            return (contenido, nombreArchivo, contentType);
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al descargar PDF de planilla: {ex.Message}");
            return null;
        }
    }
}

