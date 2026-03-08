using SistemaNominaADC.Entidades;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IDepartamentoJefaturaCliente
{
    Task<List<DepartamentoJefatura>> Lista(int? idDepartamento = null, bool soloActivos = true);
    Task<bool> Guardar(DepartamentoJefatura modelo);
    Task<bool> Desactivar(int idDepartamentoJefatura);
}

public class DepartamentoJefaturaCliente : IDepartamentoJefaturaCliente
{
    private readonly HttpClient _http;
    private readonly ApiErrorState _apiError;

    public DepartamentoJefaturaCliente(HttpClient http, ApiErrorState apiError)
    {
        _http = http;
        _apiError = apiError;
    }

    public async Task<List<DepartamentoJefatura>> Lista(int? idDepartamento = null, bool soloActivos = true)
    {
        _apiError.Clear();
        try
        {
            var url = $"api/DepartamentoJefaturas?soloActivos={soloActivos}";
            if (idDepartamento.HasValue && idDepartamento.Value > 0)
                url += $"&idDepartamento={idDepartamento.Value}";

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para consultar jefaturas.");
                return new();
            }

            return await response.Content.ReadFromJsonAsync<List<DepartamentoJefatura>>() ?? new();
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al consultar jefaturas: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> Guardar(DepartamentoJefatura modelo)
    {
        _apiError.Clear();
        if (!_apiError.TryValidateModel(modelo, "Los datos de jefatura son obligatorios.")) return false;

        try
        {
            HttpResponseMessage response = modelo.IdDepartamentoJefatura == 0
                ? await _http.PostAsJsonAsync("api/DepartamentoJefaturas", modelo)
                : await _http.PutAsJsonAsync($"api/DepartamentoJefaturas/{modelo.IdDepartamentoJefatura}", modelo);

            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para guardar jefaturas.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al guardar jefatura: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Desactivar(int idDepartamentoJefatura)
    {
        _apiError.Clear();
        if (!_apiError.TryValidatePositiveId(idDepartamentoJefatura, "id de jefatura")) return false;

        try
        {
            var response = await _http.DeleteAsync($"api/DepartamentoJefaturas/{idDepartamentoJefatura}");
            if (!response.IsSuccessStatusCode)
            {
                await response.SetApiErrorAsync(_apiError, "No autorizado para desactivar jefaturas.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _apiError.SetError($"Error al desactivar jefatura: {ex.Message}");
            return false;
        }
    }
}
