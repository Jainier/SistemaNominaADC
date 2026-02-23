using SistemaNominaADC.Entidades;
using System.Net;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface ITipoIncapacidadCliente
{
    Task<List<TipoIncapacidad>> Lista();
    Task<bool> Guardar(TipoIncapacidad modelo);
    Task<bool> Desactivar(int id);
}

public class TipoIncapacidadCliente : ITipoIncapacidadCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public TipoIncapacidadCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<TipoIncapacidad>> Lista()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/TipoIncapacidad");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar tipos de incapacidad.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<TipoIncapacidad>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar tipos de incapacidad: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(TipoIncapacidad modelo)
    {
        _apiError.Clear();
        if (!ValidarModelo(modelo)) return false;

        try
        {
            HttpResponseMessage response = modelo.IdTipoIncapacidad == 0
                ? await _http.PostAsJsonAsync("api/TipoIncapacidad", modelo)
                : await _http.PutAsJsonAsync($"api/TipoIncapacidad/{modelo.IdTipoIncapacidad}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para guardar tipos de incapacidad.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el tipo de incapacidad: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (id <= 0)
        {
            _apiError.SetError("El id del tipo de incapacidad es inválido.");
            return false;
        }

        try
        {
            var response = await _http.DeleteAsync($"api/TipoIncapacidad/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para desactivar tipos de incapacidad.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el tipo de incapacidad: {ex.Message}");
            return false;
        }
    }

    private bool ValidarModelo(TipoIncapacidad modelo)
    {
        if (modelo is null)
        {
            _apiError.SetError("Los datos del tipo de incapacidad son obligatorios.");
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
