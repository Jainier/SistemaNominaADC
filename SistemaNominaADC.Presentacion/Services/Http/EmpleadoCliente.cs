using SistemaNominaADC.Entidades;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IEmpleadoCliente
{
    Task<List<Empleado>> Lista();
    Task<bool> Guardar(Empleado modelo);
    Task<bool> Desactivar(int id);
}

public class EmpleadoCliente : IEmpleadoCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public EmpleadoCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<Empleado>> Lista()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/Empleado");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar empleados.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<Empleado>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar empleados: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(Empleado modelo)
    {
        _apiError.Clear();
        if (!_apiError.TryValidateModel(modelo, "Los datos del empleado son obligatorios.")) return false;
        try
        {
            HttpResponseMessage response = modelo.IdEmpleado == 0
                ? await _http.PostAsJsonAsync("api/Empleado", modelo)
                : await _http.PutAsJsonAsync($"api/Empleado/{modelo.IdEmpleado}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para guardar empleados.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el empleado: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(id, "id del empleado")) return false;
        try
        {
            var response = await _http.DeleteAsync($"api/Empleado/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para desactivar empleados.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el empleado: {ex.Message}");
            return false;
        }
    }
}
