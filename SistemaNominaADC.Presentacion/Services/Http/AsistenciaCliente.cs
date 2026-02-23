using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using System.Globalization;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IAsistenciaCliente
{
    Task<List<Asistencia>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
    Task<Empleado?> ObtenerMiEmpleado();
    Task<bool> RegistrarEntrada(AsistenciaMarcaDTO dto);
    Task<bool> RegistrarSalida(AsistenciaMarcaDTO dto);
}

public class AsistenciaCliente : IAsistenciaCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public AsistenciaCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<Asistencia>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        _apiError.Clear();

        try
        {
            var query = new List<string>();
            if (idEmpleado.HasValue && idEmpleado.Value > 0)
                query.Add($"idEmpleado={idEmpleado.Value}");
            if (fechaDesde.HasValue)
                query.Add($"fechaDesde={Uri.EscapeDataString(fechaDesde.Value.ToString("O", CultureInfo.InvariantCulture))}");
            if (fechaHasta.HasValue)
                query.Add($"fechaHasta={Uri.EscapeDataString(fechaHasta.Value.ToString("O", CultureInfo.InvariantCulture))}");

            var url = "api/Asistencia";
            if (query.Count > 0)
                url += "?" + string.Join("&", query);

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar asistencias.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<Asistencia>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al cargar asistencias: {ex.Message}");
            return new();
        }
    }

    public async Task<Empleado?> ObtenerMiEmpleado()
    {
        _apiError.Clear();
        try
        {
            var response = await _http.GetAsync("api/Asistencia/mi-empleado");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar el empleado actual.");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Empleado>();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al obtener el empleado actual: {ex.Message}");
            return null;
        }
    }

    public Task<bool> RegistrarEntrada(AsistenciaMarcaDTO dto) => RegistrarMarca("api/Asistencia/entrada", dto, "registrar la entrada");
    public Task<bool> RegistrarSalida(AsistenciaMarcaDTO dto) => RegistrarMarca("api/Asistencia/salida", dto, "registrar la salida");

    private async Task<bool> RegistrarMarca(string endpoint, AsistenciaMarcaDTO dto, string accion)
    {
        _apiError.Clear();

        if (!_apiError.TryValidateModel(dto, "Los datos de asistencia son obligatorios."))
            return false;

        try
        {
            var response = await _http.PostAsJsonAsync(endpoint, dto);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, $"No autorizado para {accion}.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al {accion}: {ex.Message}");
            return false;
        }
    }
}
