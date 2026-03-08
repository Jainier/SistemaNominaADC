using Microsoft.AspNetCore.Components;
using Radzen;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Helpers;
using SistemaNominaADC.Presentacion.Components.Shared;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;
using SistemaNominaADC.Negocio.Servicios;

namespace SistemaNominaADC.Presentacion.Components.Pages.Operaciones;

public partial class HorasExtra
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ISolicitudHorasExtraCliente HorasExtraCliente { get; set; } = null!;
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

    private List<SolicitudHorasExtra> solicitudes = new();
    private readonly Dictionary<int, HashSet<string>> accionesPorSolicitud = [];
    private List<Empleado> empleados = new();
    private List<TipoHoraExtra> tiposHorasExtra = new();
    private List<Estado> estados = new();

    private Empleado? empleadoActual;
    private bool esAprobador;
    private bool esGlobal;
    private int? idEmpleadoActual;
    private bool procesando;
    private bool workflowConfigurado;

    private int idEmpleadoSeleccionado;
    private DateTime fecha = DateTime.Today;
    private decimal cantidadHoras = 1;
    private int idTipoHoraExtra;
    private string motivo = string.Empty;
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

        tiposHorasExtra = (await HorasExtraCliente.TiposDisponibles())
            .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
            .ToList();
        estados = (await EstadoCliente.ListarEstadosPorEntidad(nameof(SolicitudHorasExtra)))
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
            ApiError.SetError("No hay workflow configurado para Horas Extra. Configure transiciones en Flujos de Estado Transaccionales para poder operar.");
    }

    private async Task CargarDatos()
    {
        if (filtroDesde.HasValue && filtroHasta.HasValue && filtroDesde.Value.Date > filtroHasta.Value.Date)
        {
            ApiError.SetError("La fecha inicial no puede ser mayor que la fecha final.");
            return;
        }

        solicitudes = await HorasExtraCliente.Historial(filtroIdEmpleado, filtroDesde, filtroHasta, filtroIdEstado);
        await CargarAccionesDisponibles();

        if (RegistroId.HasValue && RegistroId.Value > 0)
        {
            solicitudes = solicitudes.Where(x => x.IdSolicitudHorasExtra == RegistroId.Value).ToList();
            LimpiarRegistroIdEnUrl();
        }
    }

    private async Task CargarAccionesDisponibles()
    {
        accionesPorSolicitud.Clear();

        var tareas = solicitudes.Select(async solicitud =>
        {
            var acciones = await HorasExtraCliente.AccionesDisponibles(solicitud.IdSolicitudHorasExtra);
            accionesPorSolicitud[solicitud.IdSolicitudHorasExtra] = acciones
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
            ApiError.SetError("No hay workflow configurado para Horas Extra.");
            return;
        }

        if (idEmpleadoSeleccionado <= 0)
        {
            ApiError.SetError("Debe seleccionar un empleado.");
            return;
        }

        if (idTipoHoraExtra <= 0)
        {
            ApiError.SetError("Debe seleccionar un tipo de hora extra.");
            return;
        }

        if (cantidadHoras <= 0 || cantidadHoras > 24)
        {
            ApiError.SetError("La cantidad de horas debe estar entre 0.01 y 24.");
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
            var dto = new SolicitudHorasExtraCreateDTO
            {
                IdEmpleado = idEmpleadoSeleccionado,
                Fecha = fecha.Date,
                CantidadHoras = cantidadHoras,
                IdTipoHoraExtra = idTipoHoraExtra,
                Motivo = motivo.Trim()
            };

            var operacionOk = idSolicitudEditando.HasValue
                ? await HorasExtraCliente.Actualizar(idSolicitudEditando.Value, dto)
                : await HorasExtraCliente.Crear(dto);

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

    private async Task Aprobar(int idSolicitud)
    {
        var comentario = await SolicitarComentarioDecision("aprobar");
        if (comentario is null)
            return;

        if (await HorasExtraCliente.Aprobar(idSolicitud, comentario))
        {
            await CargarDatos();
        }
    }

    private async Task Rechazar(int idSolicitud)
    {
        var comentario = await SolicitarComentarioDecision("rechazar");
        if (comentario is null)
            return;

        if (await HorasExtraCliente.Rechazar(idSolicitud, comentario))
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

    private IEnumerable<string> AccionesWorkflow(SolicitudHorasExtra item)
    {
        if (!accionesPorSolicitud.TryGetValue(item.IdSolicitudHorasExtra, out var acciones) || acciones.Count == 0)
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

    private async Task EjecutarAccionWorkflow(SolicitudHorasExtra item, string accion)
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

        if (await HorasExtraCliente.EjecutarAccion(item.IdSolicitudHorasExtra, accionNorm, comentario))
            await CargarDatos();
    }

    private void EditarSolicitud(SolicitudHorasExtra item)
    {
        mostrarRegistro = true;
        idSolicitudEditando = item.IdSolicitudHorasExtra;
        idEmpleadoSeleccionado = item.IdEmpleado ?? 0;
        fecha = item.Fecha?.Date ?? DateTime.Today;
        cantidadHoras = item.CantidadHoras ?? 1;
        idTipoHoraExtra = item.IdTipoHoraExtra ?? 0;
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
        idSolicitudEditando = null;
        fecha = DateTime.Today;
        cantidadHoras = 1;
        idTipoHoraExtra = 0;
        motivo = string.Empty;
    }

    private void VerDetalle(SolicitudHorasExtra item)
    {
        tituloDetalleSolicitud = $"Solicitud de Horas Extra #{item.IdSolicitudHorasExtra}";
        detalleSolicitudItems = new List<SolicitudDetalleItem>
        {
            new("Empleado", item.Empleado?.NombreCompleto ?? item.IdEmpleado?.ToString()),
            new("Fecha", item.Fecha?.ToString("yyyy-MM-dd")),
            new("Cantidad horas", item.CantidadHoras?.ToString()),
            new("Tipo hora extra", item.TipoHoraExtra?.Nombre ?? item.IdTipoHoraExtra?.ToString()),
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

