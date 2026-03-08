using SistemaNominaADC.Entidades;
using System.Net;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface ITipoPlanillaConceptoCliente
{
    Task<List<TipoPlanillaConcepto>> Lista(int? idTipoPlanilla = null);
    Task<bool> Guardar(TipoPlanillaConcepto modelo);
    Task<bool> Desactivar(int idTipoPlanilla, int idConceptoNomina);
}

public class TipoPlanillaConceptoCliente : ITipoPlanillaConceptoCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public TipoPlanillaConceptoCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<TipoPlanillaConcepto>> Lista(int? idTipoPlanilla = null)
    {
        _apiError.Clear();
        try
        {
            var url = idTipoPlanilla.HasValue && idTipoPlanilla.Value > 0
                ? $"api/TipoPlanillaConcepto?idTipoPlanilla={idTipoPlanilla.Value}"
                : "api/TipoPlanillaConcepto";

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar conceptos por tipo de planilla.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<TipoPlanillaConcepto>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar conceptos por tipo de planilla: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(TipoPlanillaConcepto modelo)
    {
        _apiError.Clear();
        if (!ValidarModelo(modelo)) return false;

        try
        {
            HttpResponseMessage response = await Existe(modelo.IdTipoPlanilla, modelo.IdConceptoNomina)
                ? await _http.PutAsJsonAsync($"api/TipoPlanillaConcepto/{modelo.IdTipoPlanilla}/{modelo.IdConceptoNomina}", modelo)
                : await _http.PostAsJsonAsync("api/TipoPlanillaConcepto", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para guardar conceptos por tipo de planilla.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el concepto por tipo de planilla: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int idTipoPlanilla, int idConceptoNomina)
    {
        _apiError.Clear();
        if (idTipoPlanilla <= 0 || idConceptoNomina <= 0)
        {
            _apiError.SetError("Los ids son invalidos.");
            return false;
        }

        try
        {
            var response = await _http.DeleteAsync($"api/TipoPlanillaConcepto/Desactivar/{idTipoPlanilla}/{idConceptoNomina}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para desactivar conceptos por tipo de planilla.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el concepto por tipo de planilla: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> Existe(int idTipoPlanilla, int idConceptoNomina)
    {
        var response = await _http.GetAsync($"api/TipoPlanillaConcepto/{idTipoPlanilla}/{idConceptoNomina}");
        return response.IsSuccessStatusCode;
    }

    private bool ValidarModelo(TipoPlanillaConcepto modelo)
    {
        if (modelo is null)
        {
            _apiError.SetError("Los datos son obligatorios.");
            return false;
        }

        if (modelo.IdTipoPlanilla <= 0)
        {
            _apiError.SetError("El tipo de planilla es obligatorio.");
            return false;
        }

        if (modelo.IdConceptoNomina <= 0)
        {
            _apiError.SetError("El concepto de nomina es obligatorio.");
            return false;
        }

        if (modelo.Prioridad < 0)
        {
            _apiError.SetError("La prioridad no puede ser negativa.");
            return false;
        }

        return true;
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
