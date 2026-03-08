using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Configuracion;

public partial class TramosRentaSalario
{
    [Inject] private ITramoRentaSalarioCliente TramoCliente { get; set; } = null!;

    private List<TramoRentaSalario> listaTramos = new();
    private TramoRentaSalario tramoActual = new();
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Tramo de Renta";

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos() => listaTramos = await TramoCliente.Lista();

    private void Crear()
    {
        tramoActual = new TramoRentaSalario
        {
            VigenciaDesde = DateTime.Today,
            Activo = true
        };
        tituloFormulario = "Nuevo Tramo de Renta";
        mostrarFormulario = true;
    }

    private void Editar(TramoRentaSalario item)
    {
        tramoActual = new TramoRentaSalario
        {
            IdTramoRentaSalario = item.IdTramoRentaSalario,
            DesdeMonto = item.DesdeMonto,
            HastaMonto = item.HastaMonto,
            Tasa = item.Tasa,
            VigenciaDesde = item.VigenciaDesde,
            VigenciaHasta = item.VigenciaHasta,
            Orden = item.Orden,
            Activo = item.Activo
        };
        tituloFormulario = "Editar Tramo de Renta";
        mostrarFormulario = true;
    }

    private async Task Guardar()
    {
        if (await TramoCliente.Guardar(tramoActual))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private async Task Desactivar()
    {
        if (tramoActual.IdTramoRentaSalario > 0 && await TramoCliente.Desactivar(tramoActual.IdTramoRentaSalario))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private void Cancelar() => mostrarFormulario = false;
}
