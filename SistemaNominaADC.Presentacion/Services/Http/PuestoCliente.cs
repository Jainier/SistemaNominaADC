using SistemaNominaADC.Entidades;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IPuestoCliente
{
    Task<List<Puesto>> Lista();
    Task<bool> Guardar(Puesto modelo);
    Task<bool> Desactivar(int id);
}

public class PuestoCliente : IPuestoCliente
{
    private readonly HttpClient _http;
    public PuestoCliente(HttpClient http) => _http = http;

    public async Task<List<Puesto>> Lista() => await _http.GetFromJsonAsync<List<Puesto>>("api/Puesto") ?? new();

    public async Task<bool> Guardar(Puesto modelo)
    {
        HttpResponseMessage response = modelo.IdPuesto == 0
            ? await _http.PostAsJsonAsync("api/Puesto", modelo)
            : await _http.PutAsJsonAsync($"api/Puesto/{modelo.IdPuesto}", modelo);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> Desactivar(int id) => (await _http.DeleteAsync($"api/Puesto/Desactivar/{id}")).IsSuccessStatusCode;
}
