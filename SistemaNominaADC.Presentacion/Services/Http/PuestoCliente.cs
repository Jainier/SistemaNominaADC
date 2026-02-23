using SistemaNominaADC.Entidades;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IPuestoCliente
{
    Task<List<Puesto>> Lista();
    Task<bool> Guardar(Puesto modelo);
    Task<bool> Desactivar(int id);
}

public class PuestoCliente : IPuestoCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public PuestoCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<Puesto>> Lista()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/Puesto");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar puestos.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<Puesto>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar puestos: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(Puesto modelo)
    {
        _apiError.Clear();
        if (!_apiError.TryValidateModel(modelo, "Los datos del puesto son obligatorios.")) return false;
        try
        {
            HttpResponseMessage response = modelo.IdPuesto == 0
                ? await _http.PostAsJsonAsync("api/Puesto", modelo)
                : await _http.PutAsJsonAsync($"api/Puesto/{modelo.IdPuesto}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para guardar puestos.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el puesto: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(id, "id del puesto")) return false;
        try
        {
            var response = await _http.DeleteAsync($"api/Puesto/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para desactivar puestos.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el puesto: {ex.Message}");
            return false;
        }
    }
}
