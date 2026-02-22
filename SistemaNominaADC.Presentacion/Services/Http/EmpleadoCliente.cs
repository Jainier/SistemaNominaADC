using SistemaNominaADC.Entidades;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http;

public interface IEmpleadoCliente
{
    Task<List<Empleado>> Lista();
    Task<bool> Guardar(Empleado modelo);
    Task<bool> Desactivar(int id);
}

public class EmpleadoCliente : IEmpleadoCliente
{
    private readonly HttpClient _http;
    public EmpleadoCliente(HttpClient http) => _http = http;

    public async Task<List<Empleado>> Lista() => await _http.GetFromJsonAsync<List<Empleado>>("api/Empleado") ?? new();

    public async Task<bool> Guardar(Empleado modelo)
    {
        HttpResponseMessage response = modelo.IdEmpleado == 0
            ? await _http.PostAsJsonAsync("api/Empleado", modelo)
            : await _http.PutAsJsonAsync($"api/Empleado/{modelo.IdEmpleado}", modelo);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> Desactivar(int id) => (await _http.DeleteAsync($"api/Empleado/Desactivar/{id}")).IsSuccessStatusCode;
}
