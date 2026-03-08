using Microsoft.AspNetCore.Components;
using Radzen;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Helpers;
using SistemaNominaADC.Presentacion.Components.Shared;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Operaciones;

public partial class Permisos
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IPermisoCliente PermisoCliente { get; set; } = null!;
    [Inject] private ISolicitudesCliente SolicitudesCliente { get; set; } = null!;
    [Inject] private IAsistenciaCliente AsistenciaCliente { get; set; } = null!;
    [Inject] private IEmpleadoCliente EmpleadoCliente { get; set; } = null!;
    [Inject] private SessionService SessionService { get; set; } = null!;
    [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] protected ApiErrorState ApiError { get; set; } = null!;
    [Parameter]
    [SupplyParameterFromQuery(Name = "id")]
    public int? RegistroId { get; set; }

    private List<Permiso> solicitudes = new();
    private readonly Dictionary<int, HashSet<string>> accionesPorPermiso = [];
    private List<Empleado> empleados = new();
    private List<TipoPermiso> tiposPermiso = new();
    private List<Estado> estados = new();

    private Empleado? empleadoActual;
    private bool esAprobador;
    private bool esGlobal;
    private int? idEmpleadoActual;
    private bool procesando;
    private bool workflowConfigurado;

    private int idEmpleadoSeleccionado;
    private int idTipoPermiso;
    private DateTime fechaInicio = DateTime.Today;
    private DateTime fechaFin = DateTime.Today;
    private string motivo = string.Empty;
    private int? idPermisoEditando;
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

        tiposPermiso = (await PermisoCliente.TiposDisponibles())
            .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
            .ToList();

        estados = (await EstadoCliente.ListarEstadosPorEntidad(nameof(Permiso)))
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
            ApiError.SetError("No hay workflow configurado para Permisos. Configure transiciones en Flujos de Estado Transaccionales para poder operar.");
    }

    private async Task CargarDatos()
    {
        if (filtroDesde.HasValue && filtroHasta.HasValue && filtroDesde.Value.Date > filtroHasta.Value.Date)
        {
            ApiError.SetError("La fecha inicial no puede ser mayor que la fecha final.");
            return;
        }

        solicitudes = await PermisoCliente.Historial(filtroIdEmpleado, filtroDesde, filtroHasta, filtroIdEstado);
        await CargarAccionesDisponibles();

        if (RegistroId.HasValue && RegistroId.Value > 0)
        {
            solicitudes = solicitudes.Where(x => x.IdPermiso == RegistroId.Value).ToList();
            LimpiarRegistroIdEnUrl();
        }
    }

    private async Task CargarAccionesDisponibles()
    {
        accionesPorPermiso.Clear();

        var tareas = solicitudes.Select(async solicitud =>
        {
            var acciones = await PermisoCliente.AccionesDisponibles(solicitud.IdPermiso);
            accionesPorPermiso[solicitud.IdPermiso] = acciones
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        });

        await Task.WhenAll(tareas);
    }

    private void LimpiarRegistroIdEnUrl()
    {
        if (!RegistroId.HasValue)
            return;

        RegistroId = null;
        var urlLimpia = Navigation.GetUriWithQueryParameter("id", (int?)null);
        Navigation.NavigateTo(urlLimpia, replace: true);
    }

    private async Task CrearSolicitud()
    {
        if (procesando) return;

        if (!workflowConfigurado)
        {
            ApiError.SetError("No hay workflow configurado para Permisos.");
            return;
        }

        if (idEmpleadoSeleccionado <= 0)
        {
            ApiError.SetError("Debe seleccionar un empleado.");
            return;
        }

        if (idTipoPermiso <= 0)
        {
            ApiError.SetError("Debe seleccionar un tipo de permiso.");
            return;
        }

        if (fechaInicio.Date > fechaFin.Date)
        {
            ApiError.SetError("La fecha de inicio no puede ser mayor que la fecha fin.");
            return;
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            ApiError.SetError("El motivo es obligatorio.");
            return;
        }

        procesando = true;
        try
        {
            var dto = new PermisoCreateDTO
            {
                IdEmpleado = idEmpleadoSeleccionado,
                IdTipoPermiso = idTipoPermiso,
                FechaInicio = fechaInicio.Date,
                FechaFin = fechaFin.Date,
                Motivo = motivo.Trim()
            };

            var operacionOk = idPermisoEditando.HasValue
                ? await PermisoCliente.Actualizar(idPermisoEditando.Value, dto)
                : await PermisoCliente.Crear(dto);

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

    private async Task Aprobar(int idPermiso)
    {
        var comentario = await SolicitarComentarioDecision("aprobar");
        if (comentario is null)
            return;

        if (await PermisoCliente.Aprobar(idPermiso, comentario))
        {
            await CargarDatos();
        }
    }

    private async Task Rechazar(int idPermiso)
    {
        var comentario = await SolicitarComentarioDecision("rechazar");
        if (comentario is null)
            return;

        if (await PermisoCliente.Rechazar(idPermiso, comentario))
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

    private bool PuedeDecidirPorRelacion(int? idEmpleadoSolicitud)
    {
        if (!workflowConfigurado || !esAprobador)
            return false;

        if (esGlobal)
            return true;

        return !(idEmpleadoActual.HasValue
            && idEmpleadoSolicitud.HasValue
            && idEmpleadoActual.Value == idEmpleadoSolicitud.Value);
    }

    private bool PuedeEditarPorRelacion(int? idEmpleadoSolicitud)
    {
        if (!workflowConfigurado || !idEmpleadoSolicitud.HasValue)
            return false;

        if (esGlobal)
            return true;

        if (idEmpleadoActual.HasValue && idEmpleadoActual.Value == idEmpleadoSolicitud.Value)
            return true;

        return esAprobador && empleados.Any(e => e.IdEmpleado == idEmpleadoSolicitud.Value);
    }

    private IEnumerable<string> AccionesWorkflow(Permiso item)
    {
        if (!accionesPorPermiso.TryGetValue(item.IdPermiso, out var acciones) || acciones.Count == 0)
            return Enumerable.Empty<string>();

        var accionesFiltradas = acciones.Where(accion =>
        {
            if (string.Equals(accion, "Editar", StringComparison.OrdinalIgnoreCase))
                return PuedeEditarPorRelacion(item.IdEmpleado);

            return PuedeDecidirPorRelacion(item.IdEmpleado);
        });

        var ordenPreferido = new[] { "Editar", "Aprobar", "Rechazar" };
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

    private async Task EjecutarAccionWorkflow(Permiso item, string accion)
    {
        var accionNorm = (accion ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(accionNorm))
            return;

        if (string.Equals(accionNorm, "EDITAR", StringComparison.OrdinalIgnoreCase))
        {
            EditarSolicitud(item);
            return;
        }

        string? comentario = null;
        if (string.Equals(accionNorm, "APROBAR", StringComparison.OrdinalIgnoreCase))
        {
            comentario = await SolicitarComentarioDecision("aprobar");
            if (comentario is null)
                return;
        }

        if (string.Equals(accionNorm, "RECHAZAR", StringComparison.OrdinalIgnoreCase))
        {
            comentario = await SolicitarComentarioDecision("rechazar");
            if (comentario is null)
                return;
        }

        if (await PermisoCliente.EjecutarAccion(item.IdPermiso, accionNorm, comentario))
            await CargarDatos();
    }

    private void EditarSolicitud(Permiso item)
    {
        mostrarRegistro = true;
        idPermisoEditando = item.IdPermiso;
        idEmpleadoSeleccionado = item.IdEmpleado ?? 0;
        idTipoPermiso = item.IdTipoPermiso ?? 0;
        fechaInicio = item.FechaInicio?.Date ?? DateTime.Today;
        fechaFin = item.FechaFin?.Date ?? DateTime.Today;
        motivo = item.Motivo ?? string.Empty;
    }

    private void MostrarLista()
    {
        mostrarRegistro = false;
    }

    private void MostrarRegistro()
    {
        mostrarRegistro = true;
    }

    private void CancelarEdicion()
    {
        LimpiarFormularioSolicitud();
        mostrarRegistro = false;
    }

    private void LimpiarFormularioSolicitud()
    {
        idPermisoEditando = null;
        idTipoPermiso = 0;
        fechaInicio = DateTime.Today;
        fechaFin = DateTime.Today;
        motivo = string.Empty;
    }

    private void VerDetalle(Permiso item)
    {
        tituloDetalleSolicitud = $"Solicitud de Permiso #{item.IdPermiso}";
        detalleSolicitudItems = new List<SolicitudDetalleItem>
        {
            new("Empleado", item.Empleado?.NombreCompleto ?? item.IdEmpleado?.ToString()),
            new("Tipo permiso", item.TipoPermiso?.Nombre ?? item.IdTipoPermiso?.ToString()),
            new("Goce salarial", item.TipoPermiso is null ? null : (item.TipoPermiso.ConGoceSalarial ? "Con goce" : "Sin goce")),
            new("Fecha inicio", item.FechaInicio?.ToString("yyyy-MM-dd")),
            new("Fecha fin", item.FechaFin?.ToString("yyyy-MM-dd")),
            new("Motivo", item.Motivo),
            new("Estado", item.Estado?.Nombre),
            new("Comentario aprobacion", item.ComentarioAprobacion),
            new("Gestionado por", item.IdentityUserIdDecision)
        };
        mostrarDetalleSolicitud = true;
    }

    private void CerrarDetalle()
    {
        mostrarDetalleSolicitud = false;
    }
}

