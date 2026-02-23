using SistemaNominaADC.Entidades;
using System.Net;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface ITipoPermisoCliente
{
    Task<List<TipoPermiso>> Lista();
    Task<bool> Guardar(TipoPermiso modelo);
    Task<bool> Desactivar(int id);
}

public class TipoPermisoCliente : ITipoPermisoCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public TipoPermisoCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<TipoPermiso>> Lista()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/TipoPermiso");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar tipos de permiso.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<TipoPermiso>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar tipos de permiso: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(TipoPermiso modelo)
    {
        _apiError.Clear();
        if (!ValidarModelo(modelo)) return false;

        try
        {
            HttpResponseMessage response = modelo.IdTipoPermiso == 0
                ? await _http.PostAsJsonAsync("api/TipoPermiso", modelo)
                : await _http.PutAsJsonAsync($"api/TipoPermiso/{modelo.IdTipoPermiso}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para guardar tipos de permiso.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el tipo de permiso: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (id <= 0)
        {
            _apiError.SetError("El id del tipo de permiso es inválido.");
            return false;
        }

        try
        {
            var response = await _http.DeleteAsync($"api/TipoPermiso/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para desactivar tipos de permiso.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el tipo de permiso: {ex.Message}");
            return false;
        }
    }

    private bool ValidarModelo(TipoPermiso modelo)
    {
        if (modelo is null)
        {
            _apiError.SetError("Los datos del tipo de permiso son obligatorios.");
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
