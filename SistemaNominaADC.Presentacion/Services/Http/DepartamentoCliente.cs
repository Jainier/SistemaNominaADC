using SistemaNominaADC.Entidades;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http
{
    public interface IDepartamentoCliente
    {
        Task<List<Departamento>> Lista();
        Task<Departamento?> Obtener(int id);
        Task<bool> Guardar(Departamento modelo);
        Task<bool> Eliminar(int id);
    }

    public class DepartamentoCliente : IDepartamentoCliente
    {
        private readonly HttpClient _http;
        private readonly ApiErrorState _apiError;

        public DepartamentoCliente(HttpClient http, ApiErrorState apiError)
        {
            _http = http;
            _apiError = apiError;
        }

        public async Task<List<Departamento>> Lista()
        {
            _apiError.Clear();
            try
            {
                var response = await _http.GetAsync("api/Departamento/");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar departamentos.");
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<Departamento>>() ?? new();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar departamentos: {ex.Message}");
                return new();
            }
        }

        public async Task<Departamento?> Obtener(int id)
        {
            _apiError.Clear();
            if (!_apiError.TryValidatePositiveId(id, "id del departamento")) return null;
            try
            {
                var response = await _http.GetAsync($"api/Departamento/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar el departamento.");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<Departamento>();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar el departamento: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Guardar(Departamento depto)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateModel(depto, "Los datos del departamento son obligatorios.")) return false;
            try
            {
                HttpResponseMessage response = depto.IdDepartamento == 0
                    ? await _http.PostAsJsonAsync("api/Departamento", depto)
                    : await _http.PutAsJsonAsync($"api/departamento/{depto.IdDepartamento}", depto);

                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para guardar departamentos.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al guardar el departamento: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            _apiError.Clear();
            if (!_apiError.TryValidatePositiveId(id, "id del departamento")) return false;
            try
            {
                var response = await _http.DeleteAsync($"api/Departamento/Eliminar/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para eliminar departamentos.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al eliminar el departamento: {ex.Message}");
                return false;
            }
        }
    }
}
