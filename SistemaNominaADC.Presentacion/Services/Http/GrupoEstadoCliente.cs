using SistemaNominaADC.Entidades;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http
{
    public interface IGrupoEstadoCliente
    {
        Task<List<GrupoEstado>> Lista();
        Task<bool> Guardar(GrupoEstado entidad);
        Task<bool> Eliminar(int id);
        Task<GrupoEstado?> ObtenerPorId(int id);
    }

    public class GrupoEstadoCliente : IGrupoEstadoCliente
    {
        private readonly HttpClient _http;
        private readonly ApiErrorState _apiError;

        public GrupoEstadoCliente(HttpClient http, ApiErrorState apiError)
        {
            _http = http;
            _apiError = apiError;
        }

        public async Task<List<GrupoEstado>> Lista()
        {
            _apiError.Clear();
            try
            {
                var response = await _http.GetAsync("api/GrupoEstado/Lista");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar grupos de estado.");
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<GrupoEstado>>() ?? new();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar grupos: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> Guardar(GrupoEstado entidad)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateModel(entidad, "Los datos del grupo son obligatorios.")) return false;
            try
            {
                var response = await _http.PostAsJsonAsync("api/GrupoEstado/Guardar", entidad);
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para guardar grupos de estado.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al guardar el grupo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            _apiError.Clear();
            if (!_apiError.TryValidatePositiveId(id, "id del grupo")) return false;
            try
            {
                var response = await _http.DeleteAsync($"api/GrupoEstado/Eliminar/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para eliminar grupos de estado.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al eliminar el grupo: {ex.Message}");
                return false;
            }
        }

        public async Task<GrupoEstado?> ObtenerPorId(int id)
        {
            _apiError.Clear();
            if (!_apiError.TryValidatePositiveId(id, "id del grupo")) return null;
            try
            {
                var response = await _http.GetAsync($"api/GrupoEstado/Obtener/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar el grupo de estado.");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<GrupoEstado>();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar el grupo: {ex.Message}");
                return null;
            }
        }
    }
}
