using SistemaNominaADC.Entidades;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http
{
    public interface IObjetoSistemaCliente
    {
        Task<List<ObjetoSistema>> Lista(); 
        Task<bool> Guardar(ObjetoSistema entidad);
        Task<ObjetoSistema?> ObtenerPorNombre(string nombreEntidad);
        Task<List<ObjetoSistema>> ListaParaMenu();
    }

    public class ObjetoSistemaCliente : IObjetoSistemaCliente
    {
        private readonly HttpClient _http;
        public ObjetoSistemaCliente(HttpClient http) => _http = http;

        public async Task<List<ObjetoSistema>> Lista()
        {
            return await _http.GetFromJsonAsync<List<ObjetoSistema>>("api/ObjetosSistema/Lista") ?? new();
        }

        public async Task<bool> Guardar(ObjetoSistema entidad)
        {
            var response = await _http.PostAsJsonAsync("api/ObjetosSistema/Guardar", entidad);
            return response.IsSuccessStatusCode;
        }

        public async Task<ObjetoSistema?> ObtenerPorNombre(string nombreEntidad)
        {
            return await _http.GetFromJsonAsync<ObjetoSistema>($"api/ObjetosSistema/Obtener/{nombreEntidad}");
        }

        public async Task<List<ObjetoSistema>> ListaParaMenu()
        {
            return await _http.GetFromJsonAsync<List<ObjetoSistema>>("api/ObjetosSistema/ListaParaMenu") ?? new();
        }
    }
}
