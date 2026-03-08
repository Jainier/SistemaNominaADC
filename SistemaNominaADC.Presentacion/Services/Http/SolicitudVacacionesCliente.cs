using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using System.Globalization;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface ISolicitudVacacionesCliente
{
    Task<List<SolicitudVacaciones>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null);
    Task<List<string>> AccionesDisponibles(int idSolicitud);
    Task<bool> EjecutarAccion(int idSolicitud, string accion, string? comentario = null);
    Task<List<Estado>> EstadosDisponibles();
    Task<int?> ObtenerDiasRestantes(int idEmpleado);
    Task<bool> Crear(SolicitudVacacionesCreateDTO dto);
    Task<bool> Actualizar(int idSolicitud, SolicitudVacacionesCreateDTO dto);
    Task<bool> Aprobar(int idSolicitud, string? comentario = null);
    Task<bool> Rechazar(int idSolicitud, string motivoRechazo);
}

public class SolicitudVacacionesCliente : ISolicitudVacacionesCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public SolicitudVacacionesCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<SolicitudVacaciones>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null)
    {
        _apiError.Clear();
        try
        {
            var query = new List<string>();
            if (idEmpleado.HasValue && idEmpleado.Value > 0) query.Add($"idEmpleado={idEmpleado.Value}");
            if (fechaDesde.HasValue) query.Add($"fechaDesde={Uri.EscapeDataString(fechaDesde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
            if (fechaHasta.HasValue) query.Add($"fechaHasta={Uri.EscapeDataString(fechaHasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
            if (idEstado.HasValue && idEstado.Value > 0) query.Add($"idEstado={idEstado.Value}");

            var url = "api/SolicitudesVacaciones";
            if (query.Count > 0) url += "?" + string.Join("&", query);

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar solicitudes de vacaciones.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<SolicitudVacaciones>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar solicitudes de vacaciones: {ex.Message}");
            return new();
        }
    }

    public async Task<List<string>> AccionesDisponibles(int idSolicitud)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idSolicitud, "id de la solicitud")) return new();

        try
        {
            var response = await _http.GetAsync($"api/SolicitudesVacaciones/{idSolicitud}/acciones");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar acciones disponibles.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<string>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar acciones disponibles: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> EjecutarAccion(int idSolicitud, string accion, string? comentario = null)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idSolicitud, "id de la solicitud")) return false;
        if (!_apiError.TryValidateRequiredText(accion, "La accion es obligatoria.")) return false;

        try
        {
            var payload = new EjecutarAccionWorkflowDTO
            {
                Accion = accion.Trim(),
                Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
            };

            var response = await _http.PatchAsJsonAsync($"api/SolicitudesVacaciones/{idSolicitud}/accion", payload);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para ejecutar la accion solicitada.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al ejecutar la accion: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Estado>> EstadosDisponibles()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/SolicitudesVacaciones/estados-disponibles");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar estados.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<Estado>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar estados de vacaciones: {ex.Message}");
            return new();
        }
    }

    public async Task<int?> ObtenerDiasRestantes(int idEmpleado)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idEmpleado, "id del empleado")) return null;

        try
        {
            var response = await _http.GetAsync($"api/SolicitudesVacaciones/saldo/{idEmpleado}");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar saldo de vacaciones.");
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<SaldoVacacionesDTO>();
            return payload?.DiasRestantes ?? 0;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar saldo de vacaciones: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> Crear(SolicitudVacacionesCreateDTO dto)
    {
        _apiError.Clear();
        if (!_apiError.TryValidateModel(dto, "Los datos de la solicitud de vacaciones son obligatorios.")) return false;

        try
        {
            var response = await _http.PostAsJsonAsync("api/SolicitudesVacaciones", dto);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para crear solicitudes de vacaciones.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al crear solicitud de vacaciones: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Actualizar(int idSolicitud, SolicitudVacacionesCreateDTO dto)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idSolicitud, "id de la solicitud")) return false;
        if (!_apiError.TryValidateModel(dto, "Los datos de la solicitud de vacaciones son obligatorios.")) return false;

        try
        {
            var response = await _http.PutAsJsonAsync($"api/SolicitudesVacaciones/{idSolicitud}", dto);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para actualizar solicitudes de vacaciones.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al actualizar solicitud de vacaciones: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Aprobar(int idSolicitud, string? comentario = null)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idSolicitud, "id de la solicitud")) return false;

        try
        {
            var response = await _http.PatchAsJsonAsync($"api/SolicitudesVacaciones/{idSolicitud}/aprobar", new SolicitudDecisionDTO { Comentario = comentario });
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para aprobar solicitudes de vacaciones.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al aprobar solicitud de vacaciones: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Rechazar(int idSolicitud, string motivoRechazo)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idSolicitud, "id de la solicitud")) return false;
        if (!_apiError.TryValidateRequiredText(motivoRechazo, "El motivo de rechazo es obligatorio.")) return false;

        try
        {
            var response = await _http.PatchAsJsonAsync($"api/SolicitudesVacaciones/{idSolicitud}/rechazar", new SolicitudDecisionDTO { Comentario = motivoRechazo.Trim() });
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para rechazar solicitudes de vacaciones.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al rechazar solicitud de vacaciones: {ex.Message}");
            return false;
        }
    }

    private sealed class SaldoVacacionesDTO
    {
        public int DiasRestantes { get; set; }
    }
}
