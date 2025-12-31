using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos
{
    public partial class ObjetosSistema
    {
        [Inject] private IObjetoSistemaCliente ObjetoCliente { get; set; } = null!;
        [Inject] private IGrupoEstadoCliente GrupoCliente { get; set; } = null!; 

        private List<ObjetoSistema>? listaObjetos;
        private List<GrupoEstado>? listaGrupos;
        private ObjetoSistema objetoActual = new();
        private bool mostrarFormulario = false;
        private string tituloFormulario = "";

        protected override async Task OnInitializedAsync()
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            listaObjetos = await ObjetoCliente.Lista();
            listaGrupos = await GrupoCliente.Lista();
        }

        private void Crear()
        {
            objetoActual = new ObjetoSistema();
            tituloFormulario = "Configurar Nueva Entidad";
            mostrarFormulario = true;
        }

        private void Editar(ObjetoSistema item)
        {
            objetoActual = item;
            tituloFormulario = $"Editando Configuración: {item.NombreEntidad}";
            mostrarFormulario = true;
        }

        private async Task Guardar()
        {
            if (await ObjetoCliente.Guardar(objetoActual))
            {
                mostrarFormulario = false;
                await CargarDatos();
            }
        }

        private void Cancelar() => mostrarFormulario = false;
    }
}
