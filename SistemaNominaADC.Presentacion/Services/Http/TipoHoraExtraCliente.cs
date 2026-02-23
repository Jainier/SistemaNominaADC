using SistemaNominaADC.Entidades;
using System.Net;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface ITipoHoraExtraCliente
{
    Task<List<TipoHoraExtra>> Lista();
    Task<bool> Guardar(TipoHoraExtra modelo);
    Task<bool> Desactivar(int id);
}

public class TipoHoraExtraCliente : ITipoHoraExtraCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public TipoHoraExtraCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<TipoHoraExtra>> Lista()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/TipoHoraExtra");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar tipos de hora extra.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<TipoHoraExtra>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar tipos de hora extra: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(TipoHoraExtra modelo)
    {
        _apiError.Clear();
        if (!ValidarModelo(modelo)) return false;

        try
        {
            HttpResponseMessage response = modelo.IdTipoHoraExtra == 0
                ? await _http.PostAsJsonAsync("api/TipoHoraExtra", modelo)
                : await _http.PutAsJsonAsync($"api/TipoHoraExtra/{modelo.IdTipoHoraExtra}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para guardar tipos de hora extra.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el tipo de hora extra: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (id <= 0)
        {
            _apiError.SetError("El id del tipo de hora extra es inválido.");
            return false;
        }

        try
        {
            var response = await _http.DeleteAsync($"api/TipoHoraExtra/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para desactivar tipos de hora extra.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el tipo de hora extra: {ex.Message}");
            return false;
        }
    }

    private bool ValidarModelo(TipoHoraExtra modelo)
    {
        if (modelo is null)
        {
            _apiError.SetError("Los datos del tipo de hora extra son obligatorios.");
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

        if (!modelo.PorcentajePago.HasValue)
        {
            _apiError.SetError("El porcentaje de pago es obligatorio.");
            return false;
        }

        if (modelo.PorcentajePago.Value <= 0 || modelo.PorcentajePago.Value > 9.9999m)
        {
            _apiError.SetError("El porcentaje de pago debe ser mayor que 0 y menor o igual a 9.9999.");
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
            _apiError.SetError(string.IsNullOrWhiteSpace(error) ? "No tienes permisos para esta acción." : error);
            return;
        }

        _apiError.SetError(error);
    }
}
