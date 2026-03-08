using SistemaNominaADC.Entidades;
using System.Net;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IFlujoEstadoCliente
{
    Task<List<FlujoEstado>> Lista();
    Task<bool> Guardar(FlujoEstado modelo);
    Task<bool> Desactivar(int id);
}

public class FlujoEstadoCliente : IFlujoEstadoCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public FlujoEstadoCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<FlujoEstado>> Lista()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/FlujoEstado");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar flujos de estado.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<FlujoEstado>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar flujos de estado: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(FlujoEstado modelo)
    {
        _apiError.Clear();
        if (!ValidarModelo(modelo)) return false;

        try
        {
            HttpResponseMessage response = modelo.IdFlujoEstado == 0
                ? await _http.PostAsJsonAsync("api/FlujoEstado", modelo)
                : await _http.PutAsJsonAsync($"api/FlujoEstado/{modelo.IdFlujoEstado}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para guardar flujos de estado.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el flujo de estado: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (id <= 0)
        {
            _apiError.SetError("El id del flujo de estado es invalido.");
            return false;
        }

        try
        {
            var response = await _http.DeleteAsync($"api/FlujoEstado/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para desactivar flujos de estado.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el flujo de estado: {ex.Message}");
            return false;
        }
    }

    private bool ValidarModelo(FlujoEstado modelo)
    {
        if (modelo is null)
        {
            _apiError.SetError("Los datos del flujo de estado son obligatorios.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(modelo.Entidad))
        {
            _apiError.SetError("La entidad es obligatoria.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(modelo.Accion))
        {
            _apiError.SetError("La accion es obligatoria.");
            return false;
        }

        if (modelo.IdEstadoDestino <= 0)
        {
            _apiError.SetError("El estado destino es obligatorio.");
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
