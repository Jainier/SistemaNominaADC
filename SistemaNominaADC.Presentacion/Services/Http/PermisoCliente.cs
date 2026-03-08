using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using System.Globalization;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IPermisoCliente
{
    Task<List<Permiso>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null);
    Task<List<TipoPermiso>> TiposDisponibles();
    Task<List<string>> AccionesDisponibles(int idPermiso);
    Task<bool> Crear(PermisoCreateDTO dto);
    Task<bool> Actualizar(int idPermiso, PermisoCreateDTO dto);
    Task<bool> EjecutarAccion(int idPermiso, string accion, string? comentario = null);
    Task<bool> Aprobar(int idPermiso, string? comentario = null);
    Task<bool> Rechazar(int idPermiso, string motivoRechazo);
}

public class PermisoCliente : IPermisoCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;
    private readonly ITipoPermisoCliente _tipoPermisoCliente;

    public PermisoCliente(HttpClient http, ApiErrorState apiError, ITipoPermisoCliente tipoPermisoCliente)
    {
        _http = http;
        _apiError = apiError;
        _tipoPermisoCliente = tipoPermisoCliente;
    }

    public async Task<List<Permiso>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null)
    {
        _apiError.Clear();
        try
        {
            var query = new List<string>();
            if (idEmpleado.HasValue && idEmpleado.Value > 0) query.Add($"idEmpleado={idEmpleado.Value}");
            if (fechaDesde.HasValue) query.Add($"fechaDesde={Uri.EscapeDataString(fechaDesde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
            if (fechaHasta.HasValue) query.Add($"fechaHasta={Uri.EscapeDataString(fechaHasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
            if (idEstado.HasValue && idEstado.Value > 0) query.Add($"idEstado={idEstado.Value}");

            var url = "api/Permisos";
            if (query.Count > 0) url += "?" + string.Join("&", query);

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar permisos.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<Permiso>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar permisos: {ex.Message}");
            return new();
        }
    }

    public async Task<List<TipoPermiso>> TiposDisponibles()
    {
        _apiError.Clear();
        try
        {
            return await _tipoPermisoCliente.Lista();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar tipos de permiso: {ex.Message}");
            return new();
        }
    }

    public async Task<List<string>> AccionesDisponibles(int idPermiso)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idPermiso, "id del permiso")) return new();

        try
        {
            var response = await _http.GetAsync($"api/Permisos/{idPermiso}/acciones");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar acciones del permiso.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<string>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar acciones del permiso: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Crear(PermisoCreateDTO dto)
    {
        _apiError.Clear();
        if (!_apiError.TryValidateModel(dto, "Los datos del permiso son obligatorios.")) return false;

        try
        {
            var response = await _http.PostAsJsonAsync("api/Permisos", dto);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para crear permisos.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al crear permiso: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Actualizar(int idPermiso, PermisoCreateDTO dto)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idPermiso, "id del permiso")) return false;
        if (!_apiError.TryValidateModel(dto, "Los datos del permiso son obligatorios.")) return false;

        try
        {
            var response = await _http.PutAsJsonAsync($"api/Permisos/{idPermiso}", dto);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para actualizar permisos.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al actualizar permiso: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EjecutarAccion(int idPermiso, string accion, string? comentario = null)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idPermiso, "id del permiso")) return false;
        if (!_apiError.TryValidateRequiredText(accion, "La accion es obligatoria.")) return false;

        try
        {
            var payload = new EjecutarAccionWorkflowDTO
            {
                Accion = accion.Trim(),
                Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
            };

            var response = await _http.PatchAsJsonAsync($"api/Permisos/{idPermiso}/accion", payload);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para ejecutar la accion del permiso.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al ejecutar la accion del permiso: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Aprobar(int idPermiso, string? comentario = null)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idPermiso, "id del permiso")) return false;

        try
        {
            var response = await _http.PatchAsJsonAsync($"api/Permisos/{idPermiso}/aprobar", new SolicitudDecisionDTO { Comentario = comentario });
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para aprobar permisos.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al aprobar permiso: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Rechazar(int idPermiso, string motivoRechazo)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idPermiso, "id del permiso")) return false;
        if (!_apiError.TryValidateRequiredText(motivoRechazo, "El motivo de rechazo es obligatorio.")) return false;

        try
        {
            var response = await _http.PatchAsJsonAsync($"api/Permisos/{idPermiso}/rechazar", new SolicitudDecisionDTO { Comentario = motivoRechazo.Trim() });
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para rechazar permisos.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al rechazar permiso: {ex.Message}");
            return false;
        }
    }
}
