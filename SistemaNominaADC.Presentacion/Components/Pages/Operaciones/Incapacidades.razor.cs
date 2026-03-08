using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Radzen;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Helpers;
using SistemaNominaADC.Presentacion.Components.Shared;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;
using SistemaNominaADC.Negocio.Servicios;

namespace SistemaNominaADC.Presentacion.Components.Pages.Operaciones;

public partial class Incapacidades
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] protected IIncapacidadCliente IncapacidadCliente { get; set; } = null!;
    [Inject] private ISolicitudesCliente SolicitudesCliente { get; set; } = null!;
    [Inject] private IAsistenciaCliente AsistenciaCliente { get; set; } = null!;
    [Inject] private IEmpleadoCliente EmpleadoCliente { get; set; } = null!;
    [Inject] private ITipoIncapacidadCliente TipoIncapacidadCliente { get; set; } = null!;
    [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;
    [Inject] private SessionService SessionService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] protected ApiErrorState ApiError { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "id")]
    public int? RegistroId { get; set; }

    private List<Incapacidad> incapacidades = new();
    private readonly Dictionary<int, HashSet<string>> accionesPorIncapacidad = [];
    private List<Empleado> empleados = new();
    private List<TipoIncapacidad> tiposIncapacidad = new();
    private List<Estado> estados = new();

    private Empleado? empleadoActual;
    private bool esAprobador;
    private bool esGlobal;
    private int? idEmpleadoActual;
    private bool procesando;
    private bool workflowConfigurado;

    private int idEmpleadoSeleccionado;
    private int idTipoIncapacidad;
    private DateTime fechaInicio = DateTime.Today;
    private DateTime fechaFin = DateTime.Today;
    private decimal? montoCubierto;
    private string comentarioSolicitud = string.Empty;
    private byte[]? adjuntoBytes;
    private string? nombreAdjunto;
    private int? idIncapacidadEditando;
    private bool mostrarDetalleSolicitud;
    private string tituloDetalleSolicitud = "Detalle de solicitud";
    private List<SolicitudDetalleItem> detalleSolicitudItems = new();

    private int? filtroIdEmpleado;
    private DateTime? filtroDesde = FiltroFechasHelper.PrimerDiaMesActual();
    private DateTime? filtroHasta = FiltroFechasHelper.UltimoDiaMesActual();
    private int? filtroIdEstado;
    private bool mostrarRegistro;

    protected override async Task OnInitializedAsync()
    {
        var idsActivos = EstadoActivoFiltro.ObtenerIdsActivos(await EstadoCliente.Lista());

        await SessionService.WaitForInitialRestoreAsync();

        var alcance = await SolicitudesCliente.ObtenerAlcance();
        var esGlobalPorRol = SessionService.Roles.Any(r =>
            string.Equals(r, RolesSistema.Administrador, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, RolesSistema.RRHH, StringComparison.OrdinalIgnoreCase));

        esAprobador = (alcance?.EsAprobador ?? false) || esGlobalPorRol;
        esGlobal = (alcance?.EsGlobal ?? false) || esGlobalPorRol;
        idEmpleadoActual = alcance?.IdEmpleadoActual;

        tiposIncapacidad = (await IncapacidadCliente.TiposDisponibles())
            .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
            .ToList();
        if (tiposIncapacidad.Count == 0)
            tiposIncapacidad = (await TipoIncapacidadCliente.Lista())
                .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
                .ToList();

        estados = (await EstadoCliente.ListarEstadosPorEntidad(nameof(Incapacidad)))
            .Where(x => x.EstadoActivo != false)
            .Where(x =>
                x.Codigo == EstadoCodigosSistema.Pendiente ||
                x.Codigo == EstadoCodigosSistema.Aprobado ||
                x.Codigo == EstadoCodigosSistema.Rechazado)
            .OrderBy(x => x.Codigo)
            .ToList();
        workflowConfigurado = estados.Count > 0;

        if (esAprobador)
        {
            empleados = (await SolicitudesCliente.EmpleadosGestionables())
                .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
                .ToList();

            if (empleados.Count == 0 && esGlobal)
            {
                empleados = (await EmpleadoCliente.Lista())
                    .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
                    .ToList();
            }

            if (empleados.Count == 0)
            {
                empleadoActual = await AsistenciaCliente.ObtenerMiEmpleado();
                if (empleadoActual is not null)
                {
                    empleados = new List<Empleado> { empleadoActual };
                    idEmpleadoSeleccionado = empleadoActual.IdEmpleado;
                    filtroIdEmpleado = empleadoActual.IdEmpleado;
                }
            }

            if (alcance?.IdEmpleadoActual is int idActual && idActual > 0)
            {
                idEmpleadoSeleccionado = idActual;
            }
        }
        else
        {
            empleadoActual = await AsistenciaCliente.ObtenerMiEmpleado();
            if (empleadoActual is not null)
            {
                empleados = new List<Empleado> { empleadoActual };
                idEmpleadoSeleccionado = empleadoActual.IdEmpleado;
                filtroIdEmpleado = empleadoActual.IdEmpleado;
            }
        }

        await CargarDatos();

        if (!workflowConfigurado)
            ApiError.SetError("No hay workflow configurado para Incapacidades. Configure transiciones en Flujos de Estado Transaccionales para poder operar.");
    }

    private async Task CargarDatos()
    {
        if (filtroDesde.HasValue && filtroHasta.HasValue && filtroDesde.Value.Date > filtroHasta.Value.Date)
        {
            ApiError.SetError("La fecha inicial no puede ser mayor que la fecha final.");
            return;
        }

        incapacidades = await IncapacidadCliente.Historial(filtroIdEmpleado, filtroDesde, filtroHasta, filtroIdEstado);
        await CargarAccionesDisponibles();

        if (RegistroId.HasValue && RegistroId.Value > 0)
        {
            incapacidades = incapacidades.Where(x => x.IdIncapacidad == RegistroId.Value).ToList();
            LimpiarRegistroIdEnUrl();
        }
    }

    private async Task CargarAccionesDisponibles()
    {
        accionesPorIncapacidad.Clear();

        var tareas = incapacidades.Select(async incapacidad =>
        {
            var acciones = await IncapacidadCliente.AccionesDisponibles(incapacidad.IdIncapacidad);
            accionesPorIncapacidad[incapacidad.IdIncapacidad] = acciones
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        });

        await Task.WhenAll(tareas);
    }

    private async Task SeleccionarAdjunto(InputFileChangeEventArgs e)
    {
        var archivo = e.File;
        if (archivo is null)
            return;

        const long maxBytes = 5 * 1024 * 1024;
        if (archivo.Size > maxBytes)
        {
            ApiError.SetError("El archivo no puede superar 5MB.");
            return;
        }

        var extension = Path.GetExtension(archivo.Name).ToLowerInvariant();
        if (extension is not ".pdf" and not ".jpg" and not ".jpeg" and not ".png")
        {
            ApiError.SetError("Solo se permiten archivos PDF, JPG o PNG.");
            return;
        }

        using var ms = new MemoryStream();
        await archivo.OpenReadStream(maxBytes).CopyToAsync(ms);
        adjuntoBytes = ms.ToArray();
        nombreAdjunto = archivo.Name;
    }

    private async Task CrearIncapacidad()
    {
        if (procesando) return;

        if (!workflowConfigurado)
        {
            ApiError.SetError("No hay workflow configurado para Incapacidades.");
            return;
        }

        if (idEmpleadoSeleccionado <= 0)
        {
            ApiError.SetError("Debe seleccionar un empleado.");
            return;
        }

        if (idTipoIncapacidad <= 0)
        {
            ApiError.SetError("Debe seleccionar un tipo de incapacidad.");
            return;
        }

        if (fechaInicio.Date > fechaFin.Date)
        {
            ApiError.SetError("La fecha de inicio no puede ser mayor que la fecha fin.");
            return;
        }

        procesando = true;
        try
        {
            var dto = new IncapacidadCreateDTO
            {
                IdEmpleado = idEmpleadoSeleccionado,
                IdTipoIncapacidad = idTipoIncapacidad,
                FechaInicio = fechaInicio.Date,
                FechaFin = fechaFin.Date,
                MontoCubierto = montoCubierto,
                ComentarioSolicitud = string.IsNullOrWhiteSpace(comentarioSolicitud) ? null : comentarioSolicitud.Trim()
            };

            var operacionOk = idIncapacidadEditando.HasValue
                ? await IncapacidadCliente.Actualizar(idIncapacidadEditando.Value, dto, adjuntoBytes, nombreAdjunto)
                : await IncapacidadCliente.Crear(dto, adjuntoBytes, nombreAdjunto);

            if (operacionOk)
            {
                LimpiarFormularioSolicitud();
                filtroIdEmpleado ??= idEmpleadoSeleccionado;
                await CargarDatos();
                mostrarRegistro = false;
            }
        }
        finally
        {
            procesando = false;
        }
    }

    private async Task Validar(int idIncapacidad)
    {
        var comentario = await SolicitarComentarioDecision("validar");
        if (comentario is null)
            return;

        if (await IncapacidadCliente.Validar(idIncapacidad, comentario))
        {
            await CargarDatos();
        }
    }

    private async Task Rechazar(int idIncapacidad)
    {
        var comentario = await SolicitarComentarioDecision("rechazar");
        if (comentario is null)
            return;

        if (await IncapacidadCliente.Rechazar(idIncapacidad, comentario))
        {
            await CargarDatos();
        }
    }

    private async Task<string?> SolicitarComentarioDecision(string accion)
    {
        var parametros = new Dictionary<string, object?>
        {
            ["Accion"] = accion
        };

        var opciones = new DialogOptions
        {
            Width = "520px",
            ShowClose = true,
            Resizable = false,
            Draggable = false,
            CloseDialogOnEsc = true
        };

        var result = await DialogService.OpenAsync<ComentarioDecisionDialog>(
            "Comentario de aprobacion/rechazo",
            parametros,
            opciones);

        return result as string;
    }

    private static bool EsEstadoPendiente(int? codigoEstado) =>
        codigoEstado == EstadoCodigosSistema.Pendiente ||
        codigoEstado == EstadoCodigosSistema.PendienteCalculo;

    private bool PuedeDecidir(int? codigoEstado)
    {
        if (!workflowConfigurado || !esAprobador)
            return false;

        return EsEstadoPendiente(codigoEstado);
    }

    private bool PuedeEditar(int? idEmpleadoSolicitud, int? codigoEstado)
    {
        if (!workflowConfigurado || !EsEstadoPendiente(codigoEstado) || !idEmpleadoSolicitud.HasValue)
            return false;

        if (esGlobal)
            return true;

        if (idEmpleadoActual.HasValue && idEmpleadoActual.Value == idEmpleadoSolicitud.Value)
            return true;

        return esAprobador && empleados.Any(e => e.IdEmpleado == idEmpleadoSolicitud.Value);
    }

    private IEnumerable<string> AccionesWorkflow(Incapacidad item)
    {
        if (!accionesPorIncapacidad.TryGetValue(item.IdIncapacidad, out var acciones) || acciones.Count == 0)
            return Enumerable.Empty<string>();

        var accionesFiltradas = acciones.Where(accion =>
        {
            if (string.Equals(accion, WorkflowAcciones.Editar, StringComparison.OrdinalIgnoreCase))
                return PuedeEditar(item.IdEmpleado, item.Estado?.Codigo);

            return PuedeDecidir(item.Estado?.Codigo);
        });

        var ordenPreferido = new[] { WorkflowAcciones.Editar, WorkflowAcciones.Aprobar, WorkflowAcciones.Rechazar };
        var setPreferido = new HashSet<string>(ordenPreferido, StringComparer.OrdinalIgnoreCase);

        var ordenadas = ordenPreferido.Where(a => accionesFiltradas.Contains(a, StringComparer.OrdinalIgnoreCase));
        var otras = accionesFiltradas
            .Where(a => !setPreferido.Contains(a))
            .OrderBy(a => a, StringComparer.OrdinalIgnoreCase);

        return ordenadas.Concat(otras);
    }

    private static string ClaseBotonAccion(string accion) => accion.Trim().ToUpperInvariant() switch
    {
        "EDITAR" => "btn-outline-primary",
        "APROBAR" => "btn-success",
        "RECHAZAR" => "btn-danger",
        _ => "btn-outline-secondary"
    };

    private async Task EjecutarAccionWorkflow(Incapacidad item, string accion)
    {
        var accionNorm = (accion ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(accionNorm))
            return;

        if (string.Equals(accionNorm, WorkflowAcciones.Editar, StringComparison.OrdinalIgnoreCase))
        {
            EditarSolicitud(item);
            return;
        }

        string? comentario = null;
        if (string.Equals(accionNorm, WorkflowAcciones.Aprobar, StringComparison.OrdinalIgnoreCase))
        {
            comentario = await SolicitarComentarioDecision("validar");
            if (comentario is null)
                return;
        }

        if (string.Equals(accionNorm, WorkflowAcciones.Rechazar, StringComparison.OrdinalIgnoreCase))
        {
            comentario = await SolicitarComentarioDecision("rechazar");
            if (comentario is null)
                return;
        }

        if (await IncapacidadCliente.EjecutarAccion(item.IdIncapacidad, accionNorm, comentario))
            await CargarDatos();
    }

    private void EditarSolicitud(Incapacidad item)
    {
        mostrarRegistro = true;
        idIncapacidadEditando = item.IdIncapacidad;
        idEmpleadoSeleccionado = item.IdEmpleado ?? 0;
        fechaInicio = item.FechaInicio?.Date ?? DateTime.Today;
        fechaFin = item.FechaFin?.Date ?? DateTime.Today;
        idTipoIncapacidad = item.IdTipoIncapacidad ?? 0;
        montoCubierto = item.MontoCubierto;
        comentarioSolicitud = item.ComentarioSolicitud ?? string.Empty;
        adjuntoBytes = null;
        nombreAdjunto = item.NombreDocumento;
    }

    private void CancelarEdicion()
    {
        LimpiarFormularioSolicitud();
        mostrarRegistro = false;
    }

    private void MostrarLista()
    {
        mostrarRegistro = false;
    }

    private void MostrarRegistro()
    {
        mostrarRegistro = true;
    }

    private void LimpiarRegistroIdEnUrl()
    {
        if (!RegistroId.HasValue)
            return;

        RegistroId = null;
        var urlLimpia = Navigation.GetUriWithQueryParameter("id", (int?)null);
        Navigation.NavigateTo(urlLimpia, replace: true);
    }

    private void VerDetalle(Incapacidad item)
    {
        tituloDetalleSolicitud = $"Solicitud de Incapacidad #{item.IdIncapacidad}";
        detalleSolicitudItems = new List<SolicitudDetalleItem>
        {
            new("Empleado", item.Empleado?.NombreCompleto ?? item.IdEmpleado?.ToString()),
            new("Tipo incapacidad", item.TipoIncapacidad?.Nombre ?? item.IdTipoIncapacidad?.ToString()),
            new("Fecha inicio", item.FechaInicio?.ToString("yyyy-MM-dd")),
            new("Fecha fin", item.FechaFin?.ToString("yyyy-MM-dd")),
            new("Monto cubierto", item.MontoCubierto?.ToString()),
            new("Estado", item.Estado?.Nombre),
            new("Comentario solicitud", item.ComentarioSolicitud),
            new("Comentario aprobacion", item.ComentarioAprobacion),
            new("Gestionado por", item.IdentityUserIdDecision),
            new("Adjunto", item.NombreDocumento)
        };
        mostrarDetalleSolicitud = true;
    }

    private async Task VerAdjunto(int idIncapacidad)
    {
        var archivo = await IncapacidadCliente.ObtenerAdjunto(idIncapacidad);
        if (!archivo.HasValue)
            return;

        var base64 = Convert.ToBase64String(archivo.Value.contenido);
        await JS.InvokeVoidAsync(
            "adcArchivos.abrirDesdeBase64",
            archivo.Value.nombreArchivo,
            archivo.Value.contentType,
            base64);
    }

    private void CerrarDetalle()
    {
        mostrarDetalleSolicitud = false;
    }

    private void LimpiarFormularioSolicitud()
    {
        idIncapacidadEditando = null;
        fechaInicio = DateTime.Today;
        fechaFin = DateTime.Today;
        idTipoIncapacidad = 0;
        montoCubierto = null;
        comentarioSolicitud = string.Empty;
        adjuntoBytes = null;
        nombreAdjunto = null;
    }
}

