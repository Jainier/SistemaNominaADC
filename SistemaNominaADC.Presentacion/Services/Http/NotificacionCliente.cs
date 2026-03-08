using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Auth;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface INotificacionCliente
{
    Task<List<Notificacion>> Mias(bool soloPendientes = false, int max = 50);
    Task<bool> MarcarLeida(int idNotificacion);
    Task<bool> MarcarTodasLeidas();
}

public class NotificacionCliente : INotificacionCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;
    private readonly SessionService _sessionService;

    public NotificacionCliente(HttpClient http, ApiErrorState apiError, SessionService sessionService)
    {
        _http = http;
        _apiError = apiError;
        _sessionService = sessionService;
    }

    public async Task<List<Notificacion>> Mias(bool soloPendientes = false, int max = 50)
    {
        if (!await PrepararSolicitudAutenticadaAsync())
            return new();

        try
        {
            var response = await _http.GetAsync($"api/Notificaciones/mias?soloPendientes={soloPendientes}&max={max}");
            if (!response.IsSuccessStatusCode)
            {
                // Carga de fondo: evita mostrar 401/403 global cuando el token aun no esta listo.
                if (response.StatusCode != HttpStatusCode.Unauthorized && response.StatusCode != HttpStatusCode.Forbidden)
                {
                    await response.SetApiErrorAsync(_apiError, "No se pudieron consultar notificaciones.");
                }

                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<Notificacion>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar notificaciones: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> MarcarLeida(int idNotificacion)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idNotificacion, "id de la notificacion")) return false;
        if (!await PrepararSolicitudAutenticadaAsync()) return false;

        try
        {
            var response = await _http.PatchAsync($"api/Notificaciones/{idNotificacion}/leer", null);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para actualizar la notificacion.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al actualizar notificacion: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> MarcarTodasLeidas()
    {
        _apiError.Clear();
        if (!await PrepararSolicitudAutenticadaAsync()) return false;

        try
        {
            var response = await _http.PatchAsync("api/Notificaciones/leer-todas", null);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para actualizar notificaciones.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al actualizar notificaciones: {ex.Message}");
            return false;
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
