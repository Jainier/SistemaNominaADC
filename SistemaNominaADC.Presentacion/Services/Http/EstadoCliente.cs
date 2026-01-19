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
        public EstadoCliente(HttpClient http) => _http = http;

        public async Task<List<Estado>> Lista() =>
            await _http.GetFromJsonAsync<List<Estado>>("api/Estado/Lista") ?? new();

        public async Task<Estado?> Obtener(int id) =>
            await _http.GetFromJsonAsync<Estado>($"api/Estado/Obtener/{id}");

        public async Task<bool> Guardar(Estado entidad, List<int> idsGrupos)
        {
            var request = new { Entidad = entidad, IdsGrupos = idsGrupos };
            var response = await _http.PostAsJsonAsync("api/Estado/Guardar", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<int>> ObtenerIdsGruposAsociados(int idEstado) =>
            await _http.GetFromJsonAsync<List<int>>($"api/Estado/GruposAsociados/{idEstado}") ?? new();

        public async Task<bool> Eliminar(int id) =>
            (await _http.DeleteAsync($"api/Estado/Eliminar/{id}")).IsSuccessStatusCode;

        public async Task<List<Estado>> ListarEstadosPorEntidad(string nombreEntidad) =>
            await _http.GetFromJsonAsync<List<Estado>>($"api/Estado/PorEntidad/{nombreEntidad}") ?? new();
    }
}