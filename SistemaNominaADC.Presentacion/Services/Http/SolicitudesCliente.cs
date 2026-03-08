using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Services.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface ISolicitudesCliente
{
    Task<SolicitudesAlcanceDTO?> ObtenerAlcance();
    Task<List<Empleado>> EmpleadosGestionables();
}

public class SolicitudesCliente : ISolicitudesCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;
    private readonly SessionService _sessionService;

    public SolicitudesCliente(HttpClient http, ApiErrorState apiError, SessionService sessionService)
    {
        _http = http;
        _apiError = apiError;
        _sessionService = sessionService;
    }

    public async Task<SolicitudesAlcanceDTO?> ObtenerAlcance()
    {
        _apiError.Clear();
        if (!await PrepararSolicitudAutenticadaAsync())
            return null;

        try
        {
            var response = await _http.GetAsync("api/Solicitudes/alcance");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar el alcance de solicitudes.");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<SolicitudesAlcanceDTO>();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar alcance de solicitudes: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Empleado>> EmpleadosGestionables()
    {
        _apiError.Clear();
        if (!await PrepararSolicitudAutenticadaAsync())
            return new();

        try
        {
            var response = await _http.GetAsync("api/Solicitudes/empleados-gestionables");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar empleados gestionables.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<Empleado>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar empleados gestionables: {ex.Message}");
            return new();
        }
    }

    private async Task<bool> PrepararSolicitudAutenticadaAsync()
    {
        await _sessionService.WaitForInitialRestoreAsync();

        if (!_sessionService.IsAuthenticated || string.IsNullOrWhiteSpace(_sessionService.Token))
            return false;

        _http.DefaultRequestHeaders.Authorization = null;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _sessionService.Token);
        return true;
    }
}
