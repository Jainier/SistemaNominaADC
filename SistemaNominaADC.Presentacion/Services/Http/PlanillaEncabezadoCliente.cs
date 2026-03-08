using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IPlanillaEncabezadoCliente
{
    Task<List<PlanillaEncabezado>> Lista();
    Task<bool> Guardar(PlanillaEncabezado modelo);
    Task<bool> Desactivar(int id);
    Task<NominaResumenPlanillaDTO?> EjecutarAccion(int idPlanilla, string accion);
    Task<(byte[] contenido, string nombreArchivo, string contentType)?> DescargarComprobantesZip(int idPlanilla);
    Task<NominaResumenPlanillaDTO?> Resumen(int idPlanilla);
    Task<List<string>> AccionesDisponibles(int idPlanilla);
}

public class PlanillaEncabezadoCliente : IPlanillaEncabezadoCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public PlanillaEncabezadoCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<PlanillaEncabezado>> Lista()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/PlanillaEncabezado");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar planillas.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<PlanillaEncabezado>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar planillas: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(PlanillaEncabezado modelo)
    {
        _apiError.Clear();
        if (!ValidarModelo(modelo)) return false;

        try
        {
            HttpResponseMessage response = modelo.IdPlanilla == 0
                ? await _http.PostAsJsonAsync("api/PlanillaEncabezado", modelo)
                : await _http.PutAsJsonAsync($"api/PlanillaEncabezado/{modelo.IdPlanilla}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para guardar planillas.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar la planilla: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (id <= 0)
        {
            _apiError.SetError("El id de la planilla es invalido.");
            return false;
        }

        try
        {
            var response = await _http.DeleteAsync($"api/PlanillaEncabezado/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para desactivar planillas.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar la planilla: {ex.Message}");
            return false;
        }
    }

    private bool ValidarModelo(PlanillaEncabezado modelo)
    {
        if (modelo is null)
        {
            _apiError.SetError("Los datos de la planilla son obligatorios.");
            return false;
        }

        if (modelo.PeriodoInicio == default)
        {
            _apiError.SetError("El periodo inicio es obligatorio.");
            return false;
        }

        if (modelo.PeriodoFin == default)
        {
            _apiError.SetError("El periodo fin es obligatorio.");
            return false;
        }

        if (modelo.FechaPago == default)
        {
            _apiError.SetError("La fecha de pago es obligatoria.");
            return false;
        }

        if (modelo.PeriodoFin < modelo.PeriodoInicio)
        {
            _apiError.SetError("El periodo fin no puede ser menor al periodo inicio.");
            return false;
        }

        if (modelo.IdTipoPlanilla <= 0)
        {
            _apiError.SetError("El tipo de planilla es obligatorio.");
            return false;
        }

        if (modelo.PeriodoAguinaldo.HasValue && modelo.PeriodoAguinaldo.Value < 0)
        {
            _apiError.SetError("El periodo de aguinaldo no puede ser negativo.");
            return false;
        }

        return true;
    }

    public async Task<NominaResumenPlanillaDTO?> EjecutarAccion(int idPlanilla, string accion)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idPlanilla, "id de planilla")) return null;
        if (string.IsNullOrWhiteSpace(accion))
        {
            _apiError.SetError("La accion es obligatoria.");
            return null;
        }

        try
        {
            var response = await _http.PostAsJsonAsync(
                $"api/PlanillaEncabezado/{idPlanilla}/accion",
                new EjecutarAccionWorkflowDTO { Accion = accion.Trim() });

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para ejecutar accion de workflow.");
                return null;
            }

            if (response.Content.Headers.ContentLength.GetValueOrDefault() == 0)
                return null;

            return await response.Content.ReadFromJsonAsync<NominaResumenPlanillaDTO>();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al ejecutar accion de workflow: {ex.Message}");
            return null;
        }
    }

    public async Task<(byte[] contenido, string nombreArchivo, string contentType)?> DescargarComprobantesZip(int idPlanilla)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idPlanilla, "id de planilla")) return null;

        try
        {
            var response = await _http.GetAsync($"api/PlanillaEncabezado/{idPlanilla}/comprobantes-zip");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para descargar colillas.");
                return null;
            }

            var contenido = await response.Content.ReadAsByteArrayAsync();
            var nombreArchivo = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName
                ?? $"Colillas_Planilla_{idPlanilla}.zip";
            nombreArchivo = nombreArchivo.Trim('"');

            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = "application/zip";

            return (contenido, nombreArchivo, contentType);
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al descargar colillas: {ex.Message}");
            return null;
        }
    }

    public async Task<NominaResumenPlanillaDTO?> Resumen(int idPlanilla)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idPlanilla, "id de planilla")) return null;
        try
        {
            var response = await _http.GetAsync($"api/PlanillaEncabezado/{idPlanilla}/resumen");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar resumen de planilla.");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<NominaResumenPlanillaDTO>();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar el resumen de planilla: {ex.Message}");
            return null;
        }
    }

    public async Task<List<string>> AccionesDisponibles(int idPlanilla)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idPlanilla, "id de planilla")) return new();
        try
        {
            var response = await _http.GetAsync($"api/PlanillaEncabezado/{idPlanilla}/acciones");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar acciones disponibles.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<string>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar acciones disponibles: {ex.Message}");
            return new();
        }
    }

    private async Task SetApiErrorAsync(HttpResponseMessage response, string unauthorizedMessage)
    {
        var error = await response.ReadErrorMessageAsync();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _apiError.SetError(string.IsNullOrWhiteSpace(error) ? unauthorizedMessage : error);
            return;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            _apiError.SetError(string.IsNullOrWhiteSpace(error) ? "No tienes permisos para esta accion." : error);
            return;
        }

        _apiError.SetError(error);
    }
}
