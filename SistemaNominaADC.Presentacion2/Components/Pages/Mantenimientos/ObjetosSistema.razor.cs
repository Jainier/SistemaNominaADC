using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos
{
    public partial class ObjetosSistema
    {
        [Inject] private IObjetoSistemaService ObjetoService { get; set; } = null!;
        [Inject] private IGrupoEstadoService GrupoService { get; set; } = null!; 

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
            listaObjetos = await ObjetoService.Lista();
            listaGrupos = await GrupoService.Lista();
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
            if (await ObjetoService.Guardar(objetoActual))
            {
                mostrarFormulario = false;
                await CargarDatos();
            }
        }

        private void Cancelar() => mostrarFormulario = false;
    }
}
