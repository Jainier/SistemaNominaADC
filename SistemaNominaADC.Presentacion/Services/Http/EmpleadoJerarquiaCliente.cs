using SistemaNominaADC.Entidades;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IEmpleadoJerarquiaCliente
{
    Task<List<EmpleadoJerarquia>> Lista(int? idSupervisor = null, int? idEmpleado = null, bool soloActivos = true);
    Task<bool> Guardar(EmpleadoJerarquia modelo);
    Task<bool> Desactivar(int idEmpleadoJerarquia);
}

public class EmpleadoJerarquiaCliente : IEmpleadoJerarquiaCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public EmpleadoJerarquiaCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<EmpleadoJerarquia>> Lista(int? idSupervisor = null, int? idEmpleado = null, bool soloActivos = true)
    {
        _apiError.Clear();
        try
        {
            var url = $"api/EmpleadoJerarquia?soloActivos={soloActivos}";
            if (idSupervisor.HasValue && idSupervisor.Value > 0)
                url += $"&idSupervisor={idSupervisor.Value}";
            if (idEmpleado.HasValue && idEmpleado.Value > 0)
                url += $"&idEmpleado={idEmpleado.Value}";

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar organigrama.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<EmpleadoJerarquia>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar organigrama: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(EmpleadoJerarquia modelo)
    {
        _apiError.Clear();
        if (!_apiError.TryValidateModel(modelo, "Los datos del organigrama son obligatorios.")) return false;

        try
        {
            HttpResponseMessage response = modelo.IdEmpleadoJerarquia == 0
                ? await _http.PostAsJsonAsync("api/EmpleadoJerarquia", modelo)
                : await _http.PutAsJsonAsync($"api/EmpleadoJerarquia/{modelo.IdEmpleadoJerarquia}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para guardar organigrama.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar organigrama: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int idEmpleadoJerarquia)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idEmpleadoJerarquia, "id del organigrama")) return false;

        try
        {
            var response = await _http.DeleteAsync($"api/EmpleadoJerarquia/{idEmpleadoJerarquia}");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para desactivar organigrama.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar organigrama: {ex.Message}");
            return false;
        }
    }
}
