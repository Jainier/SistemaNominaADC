using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using System.Globalization;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IIncapacidadCliente
{
    Task<List<Incapacidad>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null);
    Task<List<string>> AccionesDisponibles(int idIncapacidad);
    Task<bool> EjecutarAccion(int idIncapacidad, string accion, string? comentario = null);
    Task<List<TipoIncapacidad>> TiposDisponibles();
    Task<List<Estado>> EstadosDisponibles();
    Task<bool> Crear(IncapacidadCreateDTO dto, byte[]? archivoBytes, string? nombreArchivo);
    Task<bool> Actualizar(int idIncapacidad, IncapacidadCreateDTO dto, byte[]? archivoBytes, string? nombreArchivo);
    Task<bool> Validar(int idIncapacidad, string? comentario = null);
    Task<bool> Rechazar(int idIncapacidad, string motivoRechazo);
    Task<(byte[] contenido, string nombreArchivo, string contentType)?> ObtenerAdjunto(int idIncapacidad);
    string UrlDescargaAdjunto(int idIncapacidad);
}

public class IncapacidadCliente : IIncapacidadCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;
    private readonly ITipoIncapacidadCliente _tipoIncapacidadCliente;

    public IncapacidadCliente(HttpClient http, ApiErrorState apiError, ITipoIncapacidadCliente tipoIncapacidadCliente)
    {
        _http = http;
        _apiError = apiError;
        _tipoIncapacidadCliente = tipoIncapacidadCliente;
    }

    public async Task<List<Incapacidad>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null)
    {
        _apiError.Clear();
        try
        {
            var query = new List<string>();
            if (idEmpleado.HasValue && idEmpleado.Value > 0) query.Add($"idEmpleado={idEmpleado.Value}");
            if (fechaDesde.HasValue) query.Add($"fechaDesde={Uri.EscapeDataString(fechaDesde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
            if (fechaHasta.HasValue) query.Add($"fechaHasta={Uri.EscapeDataString(fechaHasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
            if (idEstado.HasValue && idEstado.Value > 0) query.Add($"idEstado={idEstado.Value}");

            var url = "api/Incapacidades";
            if (query.Count > 0) url += "?" + string.Join("&", query);

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar incapacidades.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<Incapacidad>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar incapacidades: {ex.Message}");
            return new();
        }
    }

    public async Task<List<string>> AccionesDisponibles(int idIncapacidad)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idIncapacidad, "id de incapacidad")) return new();

        try
        {
            var response = await _http.GetAsync($"api/Incapacidades/{idIncapacidad}/acciones");
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

    public async Task<bool> EjecutarAccion(int idIncapacidad, string accion, string? comentario = null)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idIncapacidad, "id de incapacidad")) return false;
        if (!_apiError.TryValidateRequiredText(accion, "La accion es obligatoria.")) return false;

        try
        {
            var payload = new EjecutarAccionWorkflowDTO
            {
                Accion = accion.Trim(),
                Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
            };

            var response = await _http.PatchAsJsonAsync($"api/Incapacidades/{idIncapacidad}/accion", payload);
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

    public async Task<List<TipoIncapacidad>> TiposDisponibles()
    {
        _apiError.Clear();
        try
        {
            return await _tipoIncapacidadCliente.Lista();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar tipos de incapacidad: {ex.Message}");
            return new();
        }
    }

    public async Task<List<Estado>> EstadosDisponibles()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/Incapacidades/estados-disponibles");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar estados de incapacidad.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<Estado>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar estados de incapacidad: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Crear(IncapacidadCreateDTO dto, byte[]? archivoBytes, string? nombreArchivo)
    {
        _apiError.Clear();
        if (!_apiError.TryValidateModel(dto, "Los datos de la incapacidad son obligatorios.")) return false;

        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.IdEmpleado.ToString(CultureInfo.InvariantCulture)), nameof(dto.IdEmpleado));
            content.Add(new StringContent(dto.FechaInicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), nameof(dto.FechaInicio));
            content.Add(new StringContent(dto.FechaFin.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), nameof(dto.FechaFin));
            content.Add(new StringContent(dto.IdTipoIncapacidad.ToString(CultureInfo.InvariantCulture)), nameof(dto.IdTipoIncapacidad));

            if (dto.MontoCubierto.HasValue)
                content.Add(new StringContent(dto.MontoCubierto.Value.ToString(CultureInfo.InvariantCulture)), nameof(dto.MontoCubierto));

            if (!string.IsNullOrWhiteSpace(dto.ComentarioSolicitud))
                content.Add(new StringContent(dto.ComentarioSolicitud.Trim()), nameof(dto.ComentarioSolicitud));

            if (archivoBytes is { Length: > 0 } && !string.IsNullOrWhiteSpace(nombreArchivo))
            {
                var fileContent = new ByteArrayContent(archivoBytes);
                content.Add(fileContent, "Adjunto", nombreArchivo);
            }

            var response = await _http.PostAsync("api/Incapacidades", content);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para registrar incapacidades.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al registrar incapacidad: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Actualizar(int idIncapacidad, IncapacidadCreateDTO dto, byte[]? archivoBytes, string? nombreArchivo)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idIncapacidad, "id de incapacidad")) return false;
        if (!_apiError.TryValidateModel(dto, "Los datos de la incapacidad son obligatorios.")) return false;

        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.IdEmpleado.ToString(CultureInfo.InvariantCulture)), nameof(dto.IdEmpleado));
            content.Add(new StringContent(dto.FechaInicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), nameof(dto.FechaInicio));
            content.Add(new StringContent(dto.FechaFin.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), nameof(dto.FechaFin));
            content.Add(new StringContent(dto.IdTipoIncapacidad.ToString(CultureInfo.InvariantCulture)), nameof(dto.IdTipoIncapacidad));

            if (dto.MontoCubierto.HasValue)
                content.Add(new StringContent(dto.MontoCubierto.Value.ToString(CultureInfo.InvariantCulture)), nameof(dto.MontoCubierto));

            if (!string.IsNullOrWhiteSpace(dto.ComentarioSolicitud))
                content.Add(new StringContent(dto.ComentarioSolicitud.Trim()), nameof(dto.ComentarioSolicitud));

            if (archivoBytes is { Length: > 0 } && !string.IsNullOrWhiteSpace(nombreArchivo))
            {
                var fileContent = new ByteArrayContent(archivoBytes);
                content.Add(fileContent, "Adjunto", nombreArchivo);
            }

            var response = await _http.PutAsync($"api/Incapacidades/{idIncapacidad}", content);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para actualizar incapacidades.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al actualizar incapacidad: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Validar(int idIncapacidad, string? comentario = null)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idIncapacidad, "id de incapacidad")) return false;

        try
        {
            var response = await _http.PatchAsJsonAsync($"api/Incapacidades/{idIncapacidad}/validar", new SolicitudDecisionDTO { Comentario = comentario });
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para validar incapacidades.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al validar incapacidad: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Rechazar(int idIncapacidad, string motivoRechazo)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idIncapacidad, "id de incapacidad")) return false;
        if (!_apiError.TryValidateRequiredText(motivoRechazo, "El motivo de rechazo es obligatorio.")) return false;

        try
        {
            var response = await _http.PatchAsJsonAsync($"api/Incapacidades/{idIncapacidad}/rechazar", new SolicitudDecisionDTO { Comentario = motivoRechazo.Trim() });
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para rechazar incapacidades.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al rechazar incapacidad: {ex.Message}");
            return false;
        }
    }

    public async Task<(byte[] contenido, string nombreArchivo, string contentType)?> ObtenerAdjunto(int idIncapacidad)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idIncapacidad, "id de incapacidad"))
            return null;

        try
        {
            var response = await _http.GetAsync($"api/Incapacidades/{idIncapacidad}/adjunto");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No fue posible abrir el adjunto.");
                return null;
            }

            var contenido = await response.Content.ReadAsByteArrayAsync();
            var nombreArchivo = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName
                ?? $"incapacidad_{idIncapacidad}.bin";
            nombreArchivo = nombreArchivo.Trim('"');

            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = "application/octet-stream";

            return (contenido, nombreArchivo, contentType);
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al abrir adjunto de incapacidad: {ex.Message}");
            return null;
        }
    }

    public string UrlDescargaAdjunto(int idIncapacidad)
    {
        var relativa = $"api/Incapacidades/{idIncapacidad}/adjunto";
        return _http.BaseAddress is null
            ? relativa
            : new Uri(_http.BaseAddress, relativa).ToString();
    }
}
