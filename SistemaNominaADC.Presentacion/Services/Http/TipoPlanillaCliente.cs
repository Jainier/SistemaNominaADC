using SistemaNominaADC.Entidades;
using System.Net;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface ITipoPlanillaCliente
{
    Task<List<TipoPlanilla>> Lista(bool soloActivos = false);
    Task<bool> Guardar(TipoPlanilla modelo);
    Task<bool> Desactivar(int id);
}

public class TipoPlanillaCliente : ITipoPlanillaCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public TipoPlanillaCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<TipoPlanilla>> Lista(bool soloActivos = false)
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync($"api/TipoPlanilla?soloActivos={soloActivos.ToString().ToLowerInvariant()}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar tipos de planilla.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<TipoPlanilla>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar tipos de planilla: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(TipoPlanilla modelo)
    {
        _apiError.Clear();
        if (!ValidarModelo(modelo)) return false;

        try
        {
            HttpResponseMessage response = modelo.IdTipoPlanilla == 0
                ? await _http.PostAsJsonAsync("api/TipoPlanilla", modelo)
                : await _http.PutAsJsonAsync($"api/TipoPlanilla/{modelo.IdTipoPlanilla}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para guardar tipos de planilla.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el tipo de planilla: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (id <= 0)
        {
            _apiError.SetError("El id del tipo de planilla es invalido.");
            return false;
        }

        try
        {
            var response = await _http.DeleteAsync($"api/TipoPlanilla/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para desactivar tipos de planilla.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el tipo de planilla: {ex.Message}");
            return false;
        }
    }

    private bool ValidarModelo(TipoPlanilla modelo)
    {
        if (modelo is null)
        {
            _apiError.SetError("Los datos del tipo de planilla son obligatorios.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(modelo.Nombre))
        {
            _apiError.SetError("El nombre es obligatorio.");
            return false;
        }

        if (modelo.Nombre.Length > 100)
        {
            _apiError.SetError("El nombre no debe exceder 100 caracteres.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(modelo.Descripcion) && modelo.Descripcion.Length > 100)
        {
            _apiError.SetError("La descripcion no debe exceder 100 caracteres.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(modelo.ModoCalculo))
        {
            _apiError.SetError("El modo de calculo es obligatorio.");
            return false;
        }

        if (modelo.IdEstado <= 0)
        {
            _apiError.SetError("El estado es obligatorio.");
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
