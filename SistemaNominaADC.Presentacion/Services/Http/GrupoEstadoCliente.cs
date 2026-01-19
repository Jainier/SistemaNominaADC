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
        public GrupoEstadoCliente(HttpClient http) => _http = http;

        public async Task<List<GrupoEstado>> Lista()
        {
            return await _http.GetFromJsonAsync<List<GrupoEstado>>("api/GrupoEstado/Lista") ?? new();

        }
        public async Task<bool> Guardar(GrupoEstado entidad)
        {
            return (await _http.PostAsJsonAsync("api/GrupoEstado/Guardar", entidad)).IsSuccessStatusCode;

        }

        public async Task<bool> Eliminar(int id)
        {
            return (await _http.DeleteAsync($"api/GrupoEstado/Eliminar/{id}")).IsSuccessStatusCode;

        }

        public async Task<GrupoEstado?> ObtenerPorId(int id)
        {
            return await _http.GetFromJsonAsync<GrupoEstado>($"api/GrupoEstado/Obtener/{id}");
        }
    }
}