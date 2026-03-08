using SistemaNominaADC.Entidades;
using System.Net;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface ITramoRentaSalarioCliente
{
    Task<List<TramoRentaSalario>> Lista();
    Task<bool> Guardar(TramoRentaSalario modelo);
    Task<bool> Desactivar(int id);
}

public class TramoRentaSalarioCliente : ITramoRentaSalarioCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public TramoRentaSalarioCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<TramoRentaSalario>> Lista()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/TramoRentaSalario");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para consultar tramos de renta.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<TramoRentaSalario>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar tramos de renta: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(TramoRentaSalario modelo)
    {
        _apiError.Clear();
        if (!ValidarModelo(modelo)) return false;

        try
        {
            HttpResponseMessage response = modelo.IdTramoRentaSalario == 0
                ? await _http.PostAsJsonAsync("api/TramoRentaSalario", modelo)
                : await _http.PutAsJsonAsync($"api/TramoRentaSalario/{modelo.IdTramoRentaSalario}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para guardar tramos de renta.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar el tramo de renta: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int id)
    {
        _apiError.Clear();
        if (id <= 0)
        {
            _apiError.SetError("El id del tramo de renta es invalido.");
            return false;
        }

        try
        {
            var response = await _http.DeleteAsync($"api/TramoRentaSalario/Desactivar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await SetApiErrorAsync(response, "No autorizado para desactivar tramos de renta.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar el tramo de renta: {ex.Message}");
            return false;
        }
    }

    private bool ValidarModelo(TramoRentaSalario modelo)
    {
        if (modelo.DesdeMonto < 0m)
        {
            _apiError.SetError("El monto desde no puede ser negativo.");
            return false;
        }

        if (modelo.HastaMonto.HasValue && modelo.HastaMonto.Value <= modelo.DesdeMonto)
        {
            _apiError.SetError("El monto hasta debe ser mayor al monto desde.");
            return false;
        }

        if (modelo.Tasa < 0m || modelo.Tasa > 1m)
        {
            _apiError.SetError("La tasa debe estar entre 0 y 1.");
            return false;
        }

        if (modelo.VigenciaHasta.HasValue && modelo.VigenciaHasta.Value.Date < modelo.VigenciaDesde.Date)
        {
            _apiError.SetError("La vigencia hasta no puede ser menor a la vigencia desde.");
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
