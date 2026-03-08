using SistemaNominaADC.Entidades;
using System.Net;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IEmpleadoConceptoNominaCliente
{
    Task<List<EmpleadoConceptoNomina>> Lista();
    Task<bool> Guardar(EmpleadoConceptoNomina modelo);
    Task<bool> Desactivar(int id);
}

public class EmpleadoConceptoNominaCliente : IEmpleadoConceptoNominaCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public EmpleadoConceptoNominaCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<EmpleadoConceptoNomina>> Lista()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/EmpleadoConceptoNomina");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar conceptos por empleado.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<EmpleadoConceptoNomina>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar conceptos por empleado: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(EmpleadoConceptoNomina modelo)
    {
        _apiError.Clear();
        if (!ValidarModelo(modelo)) return false;

        try
        {
            HttpResponseMessage response = modelo.IdEmpleadoConceptoNomina == 0
                ? await _http.PostAsJsonAsync("api/EmpleadoConceptoNomina", modelo)
                : await _http.PutAsJsonAsync($"api/EmpleadoConceptoNomina/{modelo.IdEmpleadoConceptoNomina}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para guardar conceptos por empleado.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el concepto por empleado: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (id <= 0)
        {
            _apiError.SetError("El id es invalido.");
            return false;
        }

        try
        {
            var response = await _http.DeleteAsync($"api/EmpleadoConceptoNomina/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para desactivar conceptos por empleado.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el concepto por empleado: {ex.Message}");
            return false;
        }
    }

    private bool ValidarModelo(EmpleadoConceptoNomina modelo)
    {
        if (modelo is null)
        {
            _apiError.SetError("Los datos son obligatorios.");
            return false;
        }

        if (modelo.IdEmpleado <= 0)
        {
            _apiError.SetError("El empleado es obligatorio.");
            return false;
        }

        if (modelo.IdConceptoNomina <= 0)
        {
            _apiError.SetError("El concepto es obligatorio.");
            return false;
        }

        if (!modelo.MontoFijo.HasValue && !modelo.Porcentaje.HasValue)
        {
            _apiError.SetError("Debe indicar monto fijo o porcentaje.");
            return false;
        }

        if (modelo.Porcentaje.HasValue && (modelo.Porcentaje.Value < 0m || modelo.Porcentaje.Value > 1m))
        {
            _apiError.SetError("El porcentaje debe estar entre 0 y 1.");
            return false;
        }

        if (modelo.VigenciaDesde.HasValue && modelo.VigenciaHasta.HasValue &&
            modelo.VigenciaHasta.Value.Date < modelo.VigenciaDesde.Value.Date)
        {
            _apiError.SetError("La vigencia hasta no puede ser menor que vigencia desde.");
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
