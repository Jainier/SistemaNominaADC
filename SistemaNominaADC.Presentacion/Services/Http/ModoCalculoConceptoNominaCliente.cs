using SistemaNominaADC.Entidades;
using System.Net;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IModoCalculoConceptoNominaCliente
{
    Task<List<ModoCalculoConceptoNomina>> Lista();
    Task<bool> Guardar(ModoCalculoConceptoNomina modelo);
    Task<bool> Desactivar(int id);
}

public class ModoCalculoConceptoNominaCliente : IModoCalculoConceptoNominaCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public ModoCalculoConceptoNominaCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<ModoCalculoConceptoNomina>> Lista()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/ModoCalculoConceptoNomina");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar modos de calculo.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<ModoCalculoConceptoNomina>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar modos de calculo: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(ModoCalculoConceptoNomina modelo)
    {
        _apiError.Clear();
        if (!ValidarModelo(modelo)) return false;

        try
        {
            HttpResponseMessage response = modelo.IdModoCalculoConceptoNomina == 0
                ? await _http.PostAsJsonAsync("api/ModoCalculoConceptoNomina", modelo)
                : await _http.PutAsJsonAsync($"api/ModoCalculoConceptoNomina/{modelo.IdModoCalculoConceptoNomina}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para guardar modos de calculo.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el modo de calculo: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (id <= 0)
        {
            _apiError.SetError("El id del modo de calculo es invalido.");
            return false;
        }

        try
        {
            var response = await _http.DeleteAsync($"api/ModoCalculoConceptoNomina/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para desactivar modos de calculo.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el modo de calculo: {ex.Message}");
            return false;
        }
    }

    private bool ValidarModelo(ModoCalculoConceptoNomina modelo)
    {
        if (modelo is null)
        {
            _apiError.SetError("Los datos del modo de calculo son obligatorios.");
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

        if (!string.IsNullOrWhiteSpace(modelo.Descripcion) && modelo.Descripcion.Length > 250)
        {
            _apiError.SetError("La descripcion no debe exceder 250 caracteres.");
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
