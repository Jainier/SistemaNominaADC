using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Components.Shared;
using SistemaNominaADC.Presentacion.Helpers;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;
using SistemaNominaADC.Negocio.Servicios;
using Radzen;

namespace SistemaNominaADC.Presentacion.Components.Pages.Operaciones;

public partial class Vacaciones
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ISolicitudVacacionesCliente VacacionesCliente { get; set; } = null!;
    [Inject] private ISolicitudesCliente SolicitudesCliente { get; set; } = null!;
    [Inject] private IAsistenciaCliente AsistenciaCliente { get; set; } = null!;
    [Inject] private IEmpleadoCliente EmpleadoCliente { get; set; } = null!;
    [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;
    [Inject] private SessionService SessionService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] protected ApiErrorState ApiError { get; set; } = null!;
    [Parameter]
    [SupplyParameterFromQuery(Name = "id")]
    public int? RegistroId { get; set; }

    private List<SolicitudVacaciones> solicitudes = new();
    private readonly Dictionary<int, HashSet<string>> accionesPorSolicitud = [];
    private List<Empleado> empleados = new();
    private List<Estado> estados = new();

    private Empleado? empleadoActual;
    private bool esAprobador;
    private bool esGlobal;
    private int? idEmpleadoActual;
    private bool procesando;
    private bool workflowConfigurado;

    private int idEmpleadoSeleccionado;
    private int? diasRestantes;
    private DateTime fechaInicio = DateTime.Today;
    private DateTime fechaFin = DateTime.Today;
    private string comentarioSolicitud = string.Empty;
    private int? idSolicitudEditando;
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

        estados = (await EstadoCliente.ListarEstadosPorEntidad(nameof(SolicitudVacaciones)))
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

        await CargarSaldoVacaciones();
        await CargarDatos();

        if (!workflowConfigurado)
            ApiError.SetError("No hay workflow configurado para Vacaciones. Configure transiciones en Flujos de Estado Transaccionales para poder operar.");
    }

    private async Task CargarDatos()
    {
        if (filtroDesde.HasValue && filtroHasta.HasValue && filtroDesde.Value.Date > filtroHasta.Value.Date)
        {
            ApiError.SetError("La fecha inicial no puede ser mayor que la fecha final.");
            return;
        }

        solicitudes = await VacacionesCliente.Historial(filtroIdEmpleado, filtroDesde, filtroHasta, filtroIdEstado);
        await CargarAccionesDisponibles();

        if (RegistroId.HasValue && RegistroId.Value > 0)
        {
            solicitudes = solicitudes.Where(x => x.IdSolicitudVacaciones == RegistroId.Value).ToList();
            LimpiarRegistroIdEnUrl();
        }
    }

    private async Task CargarAccionesDisponibles()
    {
        accionesPorSolicitud.Clear();

        var tareas = solicitudes.Select(async solicitud =>
        {
            var acciones = await VacacionesCliente.AccionesDisponibles(solicitud.IdSolicitudVacaciones);
            accionesPorSolicitud[solicitud.IdSolicitudVacaciones] = acciones
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
            ApiError.SetError("No hay workflow configurado para Vacaciones.");
            return;
        }

        if (idEmpleadoSeleccionado <= 0)
        {
            ApiError.SetError("Debe seleccionar un empleado.");
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
            var dto = new SolicitudVacacionesCreateDTO
            {
                IdEmpleado = idEmpleadoSeleccionado,
                FechaInicio = fechaInicio.Date,
                FechaFin = fechaFin.Date,
                ComentarioSolicitud = string.IsNullOrWhiteSpace(comentarioSolicitud) ? null : comentarioSolicitud.Trim()
            };

            var operacionOk = idSolicitudEditando.HasValue
                ? await VacacionesCliente.Actualizar(idSolicitudEditando.Value, dto)
                : await VacacionesCliente.Crear(dto);

            if (operacionOk)
            {
                LimpiarFormularioSolicitud();
                filtroIdEmpleado ??= idEmpleadoSeleccionado;
                await CargarSaldoVacaciones();
                await CargarDatos();
                mostrarRegistro = false;
            }
        }
        finally
        {
            procesando = false;
        }
    }

    private async Task OnEmpleadoSeleccionadoChanged(int value)
    {
        idEmpleadoSeleccionado = value;
        await CargarSaldoVacaciones();
    }

    private async Task OnEmpleadoSeleccionadoChangedRaw(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), out var id))
        {
            await OnEmpleadoSeleccionadoChanged(id);
            return;
        }

        idEmpleadoSeleccionado = 0;
        diasRestantes = null;
    }

    private async Task CargarSaldoVacaciones()
    {
        if (idEmpleadoSeleccionado <= 0)
        {
            diasRestantes = null;
            return;
        }

        diasRestantes = await VacacionesCliente.ObtenerDiasRestantes(idEmpleadoSeleccionado);
    }

    private void EditarSolicitud(SolicitudVacaciones item)
    {
        mostrarRegistro = true;
        idSolicitudEditando = item.IdSolicitudVacaciones;
        idEmpleadoSeleccionado = item.IdEmpleado ?? 0;
        fechaInicio = item.FechaInicio?.Date ?? DateTime.Today;
        fechaFin = item.FechaFin?.Date ?? DateTime.Today;
        comentarioSolicitud = item.ComentarioSolicitud ?? string.Empty;
        _ = CargarSaldoVacaciones();
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

    private async Task Aprobar(int idSolicitud)
    {
        var comentario = await SolicitarComentarioDecision("aprobar");
        if (comentario is null)
            return;

        if (await VacacionesCliente.Aprobar(idSolicitud, comentario))
        {
            await CargarDatos();
        }
    }

    private async Task Rechazar(int idSolicitud)
    {
        var comentario = await SolicitarComentarioDecision("rechazar");
        if (comentario is null)
            return;

        if (await VacacionesCliente.Rechazar(idSolicitud, comentario))
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

    private bool PuedeDecidir(int? idEmpleadoSolicitud, int? codigoEstado)
    {
        if (!workflowConfigurado || !esAprobador || !EsEstadoPendiente(codigoEstado))
            return false;

        if (esGlobal)
            return true;

        return !(idEmpleadoActual.HasValue
            && idEmpleadoSolicitud.HasValue
            && idEmpleadoActual.Value == idEmpleadoSolicitud.Value);
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

    private IEnumerable<string> AccionesWorkflow(SolicitudVacaciones item)
    {
        if (!accionesPorSolicitud.TryGetValue(item.IdSolicitudVacaciones, out var acciones) || acciones.Count == 0)
            return Enumerable.Empty<string>();

        var accionesFiltradas = acciones.Where(accion =>
        {
            if (string.Equals(accion, WorkflowAcciones.Editar, StringComparison.OrdinalIgnoreCase))
                return PuedeEditar(item.IdEmpleado, item.Estado?.Codigo);

            return PuedeDecidir(item.IdEmpleado, item.Estado?.Codigo);
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

    private async Task EjecutarAccionWorkflow(SolicitudVacaciones item, string accion)
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
            comentario = await SolicitarComentarioDecision("aprobar");
            if (comentario is null)
                return;
        }

        if (string.Equals(accionNorm, WorkflowAcciones.Rechazar, StringComparison.OrdinalIgnoreCase))
        {
            comentario = await SolicitarComentarioDecision("rechazar");
            if (comentario is null)
                return;
        }

        if (await VacacionesCliente.EjecutarAccion(item.IdSolicitudVacaciones, accionNorm, comentario))
            await CargarDatos();
    }

    private void LimpiarFormularioSolicitud()
    {
        idSolicitudEditando = null;
        fechaInicio = DateTime.Today;
        fechaFin = DateTime.Today;
        comentarioSolicitud = string.Empty;
    }

    private void VerDetalle(SolicitudVacaciones item)
    {
        tituloDetalleSolicitud = $"Solicitud de Vacaciones #{item.IdSolicitudVacaciones}";
        detalleSolicitudItems = new List<SolicitudDetalleItem>
        {
            new("Empleado", item.Empleado?.NombreCompleto ?? item.IdEmpleado?.ToString()),
            new("Fecha inicio", item.FechaInicio?.ToString("yyyy-MM-dd")),
            new("Fecha fin", item.FechaFin?.ToString("yyyy-MM-dd")),
            new("Cantidad dias", item.CantidadDias?.ToString()),
            new("Estado", item.Estado?.Nombre),
            new("Comentario solicitud", item.ComentarioSolicitud),
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

