using SistemaNominaADC.Entidades;
using System.Net;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface ITipoConceptoNominaCliente
{
    Task<List<TipoConceptoNomina>> Lista();
    Task<bool> Guardar(TipoConceptoNomina modelo);
    Task<bool> Desactivar(int id);
}

public class TipoConceptoNominaCliente : ITipoConceptoNominaCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public TipoConceptoNominaCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<TipoConceptoNomina>> Lista()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/TipoConceptoNomina");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar conceptos de nomina.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<TipoConceptoNomina>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar conceptos de nomina: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(TipoConceptoNomina modelo)
    {
        _apiError.Clear();
        if (!ValidarModelo(modelo)) return false;

        try
        {
            HttpResponseMessage response = modelo.IdConceptoNomina == 0
                ? await _http.PostAsJsonAsync("api/TipoConceptoNomina", modelo)
                : await _http.PutAsJsonAsync($"api/TipoConceptoNomina/{modelo.IdConceptoNomina}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para guardar conceptos de nomina.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el concepto de nomina: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (id <= 0)
        {
            _apiError.SetError("El id del concepto de nomina es invalido.");
            return false;
        }

        try
        {
            var response = await _http.DeleteAsync($"api/TipoConceptoNomina/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para desactivar conceptos de nomina.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el concepto de nomina: {ex.Message}");
            return false;
        }
    }

    private bool ValidarModelo(TipoConceptoNomina modelo)
    {
        if (modelo is null)
        {
            _apiError.SetError("Los datos del concepto de nomina son obligatorios.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(modelo.Nombre))
        {
            _apiError.SetError("El nombre es obligatorio.");
            return false;
        }

        if (modelo.Nombre.Length > 150)
        {
            _apiError.SetError("El nombre no debe exceder 150 caracteres.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(modelo.FormulaCalculo) && modelo.FormulaCalculo.Length > 1000)
        {
            _apiError.SetError("La formula no debe exceder 1000 caracteres.");
            return false;
        }

        if (modelo.IdModoCalculo <= 0)
        {
            _apiError.SetError("El modo de calculo es obligatorio.");
            return false;
        }

        if (!modelo.EsIngreso && !modelo.EsDeduccion)
        {
            _apiError.SetError("Debe seleccionar si el concepto es ingreso o deduccion.");
            return false;
        }

        if (modelo.EsIngreso && modelo.EsDeduccion)
        {
            _apiError.SetError("Un concepto no puede ser ingreso y deduccion al mismo tiempo.");
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
