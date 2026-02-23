using SistemaNominaADC.Entidades;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http
{
    public interface IEstadoCliente
    {
        Task<List<Estado>> Lista();
        Task<Estado?> Obtener(int id);
        Task<bool> Guardar(Estado entidad, List<int> idsGrupos);
        Task<List<int>> ObtenerIdsGruposAsociados(int idEstado);
        Task<bool> Eliminar(int id);
        Task<List<Estado>> ListarEstadosPorEntidad(string nombreEntidad);
    }

    public class EstadoCliente : IEstadoCliente
    {
        private readonly HttpClient _http;
        private readonly ApiErrorState _apiError;

        public EstadoCliente(HttpClient http, ApiErrorState apiError)
        {
            _http = http;
            _apiError = apiError;
        }

        public async Task<List<Estado>> Lista()
        {
            _apiError.Clear();
            try
            {
                var response = await _http.GetAsync("api/Estado/Lista");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar estados.");
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<Estado>>() ?? new();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar estados: {ex.Message}");
                return new();
            }
        }

        public async Task<Estado?> Obtener(int id)
        {
            _apiError.Clear();
            if (!_apiError.TryValidatePositiveId(id, "id del estado")) return null;
            try
            {
                var response = await _http.GetAsync($"api/Estado/Obtener/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar el estado.");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<Estado>();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar el estado: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Guardar(Estado entidad, List<int> idsGrupos)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateModel(entidad, "Los datos del estado son obligatorios.")) return false;
            if (idsGrupos is null)
            {
                _apiError.SetError("La lista de grupos es obligatoria.");
                return false;
            }
            try
            {
                var request = new { Entidad = entidad, IdsGrupos = idsGrupos };
                var response = await _http.PostAsJsonAsync("api/Estado/Guardar", request);
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para guardar estados.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al guardar el estado: {ex.Message}");
                return false;
            }
        }

        public async Task<List<int>> ObtenerIdsGruposAsociados(int idEstado)
        {
            _apiError.Clear();
            if (!_apiError.TryValidatePositiveId(idEstado, "id del estado")) return new();
            try
            {
                var response = await _http.GetAsync($"api/Estado/GruposAsociados/{idEstado}");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar grupos asociados.");
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<int>>() ?? new();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar grupos asociados: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            _apiError.Clear();
            if (!_apiError.TryValidatePositiveId(id, "id del estado")) return false;
            try
            {
                var response = await _http.DeleteAsync($"api/Estado/Eliminar/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para eliminar estados.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al eliminar el estado: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Estado>> ListarEstadosPorEntidad(string nombreEntidad)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateRequiredText(nombreEntidad, "El nombre de la entidad es obligatorio.")) return new();
            try
            {
                var response = await _http.GetAsync($"api/Estado/PorEntidad/{nombreEntidad}");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar estados por entidad.");
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<Estado>>() ?? new();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar estados por entidad: {ex.Message}");
                return new();
            }
        }
    }
}
