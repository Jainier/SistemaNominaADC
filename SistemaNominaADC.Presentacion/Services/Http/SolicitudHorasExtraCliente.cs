using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using System.Globalization;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface ISolicitudHorasExtraCliente
{
    Task<List<SolicitudHorasExtra>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null);
    Task<List<string>> AccionesDisponibles(int idSolicitud);
    Task<bool> EjecutarAccion(int idSolicitud, string accion, string? comentario = null);
    Task<List<TipoHoraExtra>> TiposDisponibles();
    Task<List<Estado>> EstadosDisponibles();
    Task<bool> Crear(SolicitudHorasExtraCreateDTO dto);
    Task<bool> Actualizar(int idSolicitud, SolicitudHorasExtraCreateDTO dto);
    Task<bool> Aprobar(int idSolicitud, string? comentario = null);
    Task<bool> Rechazar(int idSolicitud, string motivoRechazo);
}

public class SolicitudHorasExtraCliente : ISolicitudHorasExtraCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;
    private readonly ITipoHoraExtraCliente _tipoHoraExtraCliente;

    public SolicitudHorasExtraCliente(HttpClient http, ApiErrorState apiError, ITipoHoraExtraCliente tipoHoraExtraCliente)
    {
        _http = http;
        _apiError = apiError;
        _tipoHoraExtraCliente = tipoHoraExtraCliente;
    }

    public async Task<List<SolicitudHorasExtra>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null)
    {
        _apiError.Clear();
        try
        {
            var query = new List<string>();
            if (idEmpleado.HasValue && idEmpleado.Value > 0) query.Add($"idEmpleado={idEmpleado.Value}");
            if (fechaDesde.HasValue) query.Add($"fechaDesde={Uri.EscapeDataString(fechaDesde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
            if (fechaHasta.HasValue) query.Add($"fechaHasta={Uri.EscapeDataString(fechaHasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
            if (idEstado.HasValue && idEstado.Value > 0) query.Add($"idEstado={idEstado.Value}");

            var url = "api/SolicitudesHorasExtra";
            if (query.Count > 0) url += "?" + string.Join("&", query);

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar solicitudes de horas extra.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<SolicitudHorasExtra>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar solicitudes de horas extra: {ex.Message}");
            return new();
        }
    }

    public async Task<List<string>> AccionesDisponibles(int idSolicitud)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idSolicitud, "id de la solicitud")) return new();

        try
        {
            var response = await _http.GetAsync($"api/SolicitudesHorasExtra/{idSolicitud}/acciones");
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

            var response = await _http.PatchAsJsonAsync($"api/SolicitudesHorasExtra/{idSolicitud}/accion", payload);
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

    public async Task<List<TipoHoraExtra>> TiposDisponibles()
    {
        _apiError.Clear();
        try
        {
            return await _tipoHoraExtraCliente.Lista();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar tipos de horas extra: {ex.Message}");
            return new();
        }
    }

    public async Task<List<Estado>> EstadosDisponibles()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/SolicitudesHorasExtra/estados-disponibles");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar estados.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<Estado>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar estados de horas extra: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Crear(SolicitudHorasExtraCreateDTO dto)
    {
        _apiError.Clear();
        if (!_apiError.TryValidateModel(dto, "Los datos de la solicitud de horas extra son obligatorios.")) return false;

        try
        {
            var response = await _http.PostAsJsonAsync("api/SolicitudesHorasExtra", dto);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para crear solicitudes de horas extra.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al crear solicitud de horas extra: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Actualizar(int idSolicitud, SolicitudHorasExtraCreateDTO dto)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idSolicitud, "id de la solicitud")) return false;
        if (!_apiError.TryValidateModel(dto, "Los datos de la solicitud de horas extra son obligatorios.")) return false;

        try
        {
            var response = await _http.PutAsJsonAsync($"api/SolicitudesHorasExtra/{idSolicitud}", dto);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para actualizar solicitudes de horas extra.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al actualizar solicitud de horas extra: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Aprobar(int idSolicitud, string? comentario = null)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idSolicitud, "id de la solicitud")) return false;

        try
        {
            var response = await _http.PatchAsJsonAsync($"api/SolicitudesHorasExtra/{idSolicitud}/aprobar", new SolicitudDecisionDTO { Comentario = comentario });
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para aprobar solicitudes de horas extra.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al aprobar solicitud de horas extra: {ex.Message}");
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
            var response = await _http.PatchAsJsonAsync($"api/SolicitudesHorasExtra/{idSolicitud}/rechazar", new SolicitudDecisionDTO { Comentario = motivoRechazo.Trim() });
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para rechazar solicitudes de horas extra.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al rechazar solicitud de horas extra: {ex.Message}");
            return false;
        }
    }
}
