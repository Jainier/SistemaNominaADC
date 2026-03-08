using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class NominaCalculator : INominaCalculator
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NominaCalculator> _logger;
    private const string TokenSb = "SB";
    private const string TokenHoraExtra = "HORA_EXTRA";
    private const string TokenAusencia = "AUSENCIA";
    private const string TokenPermisoSinGoce = "PERMISO_SIN_GOCE";
    private const string TokenIncapacidad = "INCAPACIDAD";
    private const string TokenTi = "TI";
    private const string TokenBr = "BR";
    private const string TokenTd = "TD";
    private const string TokenNeto = "NETO";
    private const string TokenCcssSem = "CCSS_SEM";
    private const string TokenCcssIvm = "CCSS_IVM";
    private const string TokenCcssBp = "CCSS_BP";
    private const string TokenRenta = "RENTA";

    public NominaCalculator(ApplicationDbContext context, ILogger<NominaCalculator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<NominaCalculoEmpleadoDTO> CalcularEmpleado(
        int idPlanilla,
        int idEmpleado,
        DateTime periodoInicio,
        DateTime periodoFin,
        int idEstadoActivo,
        int idEstadoAprobado,
        bool trazaDetallada = false)
    {
        var traceLines = trazaDetallada ? new List<string>() : null;

        void Trace(string mensaje)
        {
            if (trazaDetallada)
                traceLines!.Add(mensaje);
        }

        if (idPlanilla <= 0) throw new BusinessException("El id de planilla es invalido.");
        if (idEmpleado <= 0) throw new BusinessException("El id de empleado es invalido.");
        if (periodoFin.Date < periodoInicio.Date) throw new BusinessException("El periodo de la planilla es invalido.");

        var planilla = await _context.PlanillasEncabezado
            .AsNoTracking()
            .Include(x => x.TipoPlanilla)
            .FirstOrDefaultAsync(x => x.IdPlanilla == idPlanilla)
            ?? throw new NotFoundException("Planilla no encontrada para el calculo.");

        var tipoPlanilla = planilla.TipoPlanilla
            ?? throw new BusinessException("La planilla no tiene tipo configurado.");

        var esPlanillaExtraordinaria = string.Equals(
            (tipoPlanilla.ModoCalculo ?? string.Empty).Trim(),
            "Extraordinaria",
            StringComparison.OrdinalIgnoreCase);

        var conceptosTipo = await _context.TiposPlanillaConcepto
            .AsNoTracking()
            .Where(x => x.IdTipoPlanilla == planilla.IdTipoPlanilla && x.Activo)
            .Select(x => new { x.IdConceptoNomina, x.Obligatorio, x.Prioridad })
            .ToListAsync();

        var conceptosPermitidosTipo = conceptosTipo
            .Select(x => x.IdConceptoNomina)
            .ToHashSet();

        var empleado = await _context.Empleados
            .AsNoTracking()
            .Include(x => x.Puesto)
            .FirstOrDefaultAsync(x => x.IdEmpleado == idEmpleado)
            ?? throw new NotFoundException("Empleado no encontrado para el calculo.");

        var conceptos = await _context.TiposConceptoNomina
            .AsNoTracking()
            .Where(x => x.IdEstado == idEstadoActivo)
            .Include(x => x.ModoCalculo)
            .OrderBy(x => x.OrdenCalculo)
            .ThenBy(x => x.IdConceptoNomina)
            .ToListAsync();

        if (conceptosPermitidosTipo.Count > 0)
        {
            conceptos = conceptos
                .Where(x =>
                    conceptosPermitidosTipo.Contains(x.IdConceptoNomina) ||
                    EsConceptoTecnico(ObtenerTokenConcepto(x)))
                .ToList();
        }
        else if (esPlanillaExtraordinaria)
        {
            conceptos = conceptos
                .Where(x =>
                {
                    var token = ObtenerTokenConcepto(x);
                    return EsConceptoTecnico(token) || !EsConceptoAutomaticoPlanillaRegular(token);
                })
                .ToList();
        }

        var tramosRentaVigentes = await _context.TramosRentaSalario
            .AsNoTracking()
            .Where(x =>
                x.Activo &&
                x.VigenciaDesde.Date <= periodoFin.Date &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value.Date >= periodoInicio.Date))
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.DesdeMonto)
            .ToListAsync();

        if (conceptos.Count == 0)
            throw new BusinessException("No hay conceptos activos para calcular la nomina.");

        Trace($"[INFO] Inicio calculo | Empleado={idEmpleado} | Planilla={idPlanilla} | Periodo={periodoInicio:yyyy-MM-dd}..{periodoFin:yyyy-MM-dd} | Tipo={tipoPlanilla.Nombre} | Modo={tipoPlanilla.ModoCalculo} | Conceptos={conceptos.Count}");

        var fechaInicioPago = Max(periodoInicio.Date, empleado.FechaIngreso.Date);
        var fechaSalida = empleado.FechaSalida?.Date ?? periodoFin.Date;
        var fechaFinPago = Min(periodoFin.Date, fechaSalida);

        var diasPagables = fechaFinPago < fechaInicioPago
            ? 0
            : (fechaFinPago - fechaInicioPago).Days + 1;

        var salarioMensualBase = empleado.SalarioBase > 0m
            ? empleado.SalarioBase
            : (empleado.Puesto?.SalarioBase ?? 0m);

        var salarioDiario = salarioMensualBase / 30m;
        var salarioHora = salarioMensualBase / 240m;
        var salarioBase = Redondear(salarioDiario * diasPagables);

        Trace($"[BASE] salarioMensual={salarioMensualBase:N2} | salarioDiario={salarioDiario:N4} | salarioHora={salarioHora:N4} | diasPagables={diasPagables} | SB={salarioBase:N2}");

        var horasExtra = await _context.SolicitudesHorasExtra
            .AsNoTracking()
            .Include(x => x.TipoHoraExtra)
            .Where(x =>
                x.IdEmpleado == idEmpleado &&
                x.IdEstado == idEstadoAprobado &&
                x.Fecha.HasValue &&
                x.Fecha.Value.Date >= periodoInicio.Date &&
                x.Fecha.Value.Date <= periodoFin.Date)
            .ToListAsync();

        var montoHoraExtra = Redondear(horasExtra.Sum(x =>
            (x.CantidadHoras ?? 0m) * salarioHora * (x.TipoHoraExtra?.PorcentajePago ?? 1m)));

        Trace($"[HORAS EXTRA] registros={horasExtra.Count} | montoHE={montoHoraExtra:N2}");

        var asistenciasPeriodo = await _context.Asistencias
            .AsNoTracking()
            .Where(x =>
                x.IdEmpleado == idEmpleado &&
                x.Fecha.Date >= periodoInicio.Date &&
                x.Fecha.Date <= periodoFin.Date)
            .ToListAsync();

        var fechasConAsistencia = asistenciasPeriodo
            .Where(x => (x.HoraEntrada.HasValue || x.HoraSalida.HasValue) && x.Ausencia != true)
            .Select(x => x.Fecha.Date)
            .ToHashSet();

        var fechasAusenciaMarcada = asistenciasPeriodo
            .Where(x => x.Ausencia == true)
            .Select(x => x.Fecha.Date)
            .ToHashSet();

        var permisosPeriodo = await _context.Permisos
            .AsNoTracking()
            .Include(x => x.TipoPermiso)
            .Where(x =>
                x.IdEmpleado == idEmpleado &&
                x.IdEstado == idEstadoAprobado &&
                x.FechaInicio.HasValue &&
                x.FechaFin.HasValue &&
                x.FechaInicio.Value.Date <= periodoFin.Date &&
                x.FechaFin.Value.Date >= periodoInicio.Date)
            .ToListAsync();

        var fechasPermisoSinGoce = new HashSet<DateTime>();
        var fechasPermisoConGoce = new HashSet<DateTime>();
        foreach (var permiso in permisosPeriodo)
        {
            var fechas = ObtenerFechasRangoSolapado(periodoInicio.Date, periodoFin.Date, permiso.FechaInicio!.Value.Date, permiso.FechaFin!.Value.Date);
            if (permiso.TipoPermiso?.ConGoceSalarial == false)
                fechasPermisoSinGoce.UnionWith(fechas);
            else
                fechasPermisoConGoce.UnionWith(fechas);
        }

        var incapacidadesPeriodo = await _context.Incapacidades
            .AsNoTracking()
            .Where(x =>
                x.IdEmpleado == idEmpleado &&
                x.FechaInicio.HasValue &&
                x.FechaFin.HasValue &&
                x.FechaInicio.Value.Date <= periodoFin.Date &&
                x.FechaFin.Value.Date >= periodoInicio.Date &&
                x.IdEstado.HasValue)
            .ToListAsync();

        var estadosIncapacidadValidos = await _context.Estados
            .AsNoTracking()
            .Where(e =>
                e.Nombre == "Validada" ||
                e.Nombre == "Aprobada" ||
                e.Nombre == "Aprobado")
            .Select(e => e.IdEstado)
            .ToListAsync();

        var fechasIncapacidad = new HashSet<DateTime>();
        foreach (var incapacidad in incapacidadesPeriodo.Where(x => estadosIncapacidadValidos.Contains(x.IdEstado!.Value)))
        {
            var fechas = ObtenerFechasRangoSolapado(periodoInicio.Date, periodoFin.Date, incapacidad.FechaInicio!.Value.Date, incapacidad.FechaFin!.Value.Date);
            fechasIncapacidad.UnionWith(fechas);
        }

        var vacacionesPeriodo = await _context.SolicitudesVacaciones
            .AsNoTracking()
            .Where(x =>
                x.IdEmpleado == idEmpleado &&
                x.IdEstado == idEstadoAprobado &&
                x.FechaInicio.HasValue &&
                x.FechaFin.HasValue &&
                x.FechaInicio.Value.Date <= periodoFin.Date &&
                x.FechaFin.Value.Date >= periodoInicio.Date)
            .ToListAsync();

        var fechasVacaciones = new HashSet<DateTime>();
        foreach (var vacacion in vacacionesPeriodo)
        {
            var fechas = ObtenerFechasRangoSolapado(periodoInicio.Date, periodoFin.Date, vacacion.FechaInicio!.Value.Date, vacacion.FechaFin!.Value.Date);
            fechasVacaciones.UnionWith(fechas);
        }

        var diasAusencia = 0;
        var diasPermisoSinGoce = 0;
        var diasIncapacidad = 0;
        var fechaCursor = fechaInicioPago;
        while (fechaCursor <= fechaFinPago)
        {
            if (fechasIncapacidad.Contains(fechaCursor))
            {
                diasIncapacidad++;
            }
            else if (fechasPermisoSinGoce.Contains(fechaCursor))
            {
                diasPermisoSinGoce++;
            }
            else if (fechasPermisoConGoce.Contains(fechaCursor))
            {
                // Pagado.
            }
            else if (fechasVacaciones.Contains(fechaCursor))
            {
                // Pagado.
            }
            else if (fechasConAsistencia.Contains(fechaCursor))
            {
                // Pagado.
            }
            else if (fechasAusenciaMarcada.Contains(fechaCursor))
            {
                diasAusencia++;
            }
            else
            {
                // Si no hay marca ni justificacion aprobada, cuenta como ausencia.
                diasAusencia++;
            }

            fechaCursor = fechaCursor.AddDays(1);
        }

        var descuentoAusencia = Redondear(salarioDiario * diasAusencia);
        var descuentoPermisoSinGoce = Redondear(salarioDiario * diasPermisoSinGoce);
        var descuentoIncapacidad = Redondear(salarioDiario * diasIncapacidad);

        Trace($"[NO LABORADO] diasAusencia={diasAusencia} ({descuentoAusencia:N2}) | diasPermisoSinGoce={diasPermisoSinGoce} ({descuentoPermisoSinGoce:N2}) | diasIncapacidad={diasIncapacidad} ({descuentoIncapacidad:N2})");

        var detallesPorToken = new Dictionary<string, List<NominaConceptoDetalleDTO>>(StringComparer.OrdinalIgnoreCase);
        var cantidadPorToken = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var unidadPorToken = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        detallesPorToken[TokenSb] =
        [
            new NominaConceptoDetalleDTO
            {
                Fecha = periodoInicio.Date,
                Motivo = $"Periodo {periodoInicio:yyyy-MM-dd} a {periodoFin:yyyy-MM-dd}",
                GestionadoPor = "N/A",
                Cantidad = diasPagables,
                UnidadCantidad = "dias",
                Monto = salarioBase
            }
        ];
        cantidadPorToken[TokenSb] = diasPagables;
        unidadPorToken[TokenSb] = "dias";

        var detallesHoraExtra = horasExtra
            .Select(x =>
            {
                var horas = x.CantidadHoras ?? 0m;
                var montoEvento = Redondear(horas * salarioHora * (x.TipoHoraExtra?.PorcentajePago ?? 1m));
                return new NominaConceptoDetalleDTO
                {
                    Fecha = x.Fecha?.Date,
                    Motivo = string.IsNullOrWhiteSpace(x.Motivo) ? "Horas extra aprobadas" : x.Motivo,
                    GestionadoPor = string.IsNullOrWhiteSpace(x.IdentityUserIdDecision) ? "N/D" : x.IdentityUserIdDecision!,
                    Cantidad = horas,
                    UnidadCantidad = "horas",
                    FactorAplicado = x.TipoHoraExtra?.PorcentajePago ?? 1m,
                    Monto = montoEvento
                };
            })
            .Where(x => x.Monto > 0m)
            .ToList();
        if (detallesHoraExtra.Count > 0)
        {
            detallesPorToken[TokenHoraExtra] = detallesHoraExtra;
            cantidadPorToken[TokenHoraExtra] = detallesHoraExtra.Sum(x => x.Cantidad ?? 0m);
            unidadPorToken[TokenHoraExtra] = "horas";
        }

        var detallesAusencia = new List<NominaConceptoDetalleDTO>();
        foreach (var fecha in fechasAusenciaMarcada.OrderBy(x => x))
        {
            if (fecha < fechaInicioPago || fecha > fechaFinPago)
                continue;

            var asistencia = asistenciasPeriodo.FirstOrDefault(a => a.Fecha.Date == fecha);
            detallesAusencia.Add(new NominaConceptoDetalleDTO
            {
                Fecha = fecha,
                Motivo = string.IsNullOrWhiteSpace(asistencia?.Justificacion) ? "Ausencia registrada" : asistencia!.Justificacion!,
                GestionadoPor = "N/A",
                Cantidad = 1m,
                UnidadCantidad = "dia",
                Monto = Redondear(salarioDiario)
            });
        }

        var faltantes = new List<DateTime>();
        fechaCursor = fechaInicioPago;
        while (fechaCursor <= fechaFinPago)
        {
            var esAusenciaImplicita =
                !fechasIncapacidad.Contains(fechaCursor) &&
                !fechasPermisoSinGoce.Contains(fechaCursor) &&
                !fechasPermisoConGoce.Contains(fechaCursor) &&
                !fechasVacaciones.Contains(fechaCursor) &&
                !fechasConAsistencia.Contains(fechaCursor) &&
                !fechasAusenciaMarcada.Contains(fechaCursor);

            if (esAusenciaImplicita)
                faltantes.Add(fechaCursor);

            fechaCursor = fechaCursor.AddDays(1);
        }

        foreach (var fecha in faltantes)
        {
            detallesAusencia.Add(new NominaConceptoDetalleDTO
            {
                Fecha = fecha,
                Motivo = "Sin marca ni justificacion aprobada",
                GestionadoPor = "N/A",
                Cantidad = 1m,
                UnidadCantidad = "dia",
                Monto = Redondear(salarioDiario)
            });
        }

        if (detallesAusencia.Count > 0)
        {
            detallesPorToken[TokenAusencia] = detallesAusencia.OrderBy(x => x.Fecha).ToList();
            cantidadPorToken[TokenAusencia] = detallesAusencia.Sum(x => x.Cantidad ?? 0m);
            unidadPorToken[TokenAusencia] = "dias";
        }

        var detallesPermisoSinGoce = permisosPeriodo
            .Where(p => p.TipoPermiso?.ConGoceSalarial == false)
            .Select(p =>
            {
                var fechas = ObtenerFechasRangoSolapado(periodoInicio.Date, periodoFin.Date, p.FechaInicio!.Value.Date, p.FechaFin!.Value.Date);
                var dias = fechas.Count;
                return new NominaConceptoDetalleDTO
                {
                    Fecha = p.FechaInicio?.Date,
                    Motivo = string.IsNullOrWhiteSpace(p.Motivo)
                        ? $"Permiso sin goce ({p.FechaInicio:yyyy-MM-dd} a {p.FechaFin:yyyy-MM-dd})"
                        : p.Motivo!,
                    GestionadoPor = string.IsNullOrWhiteSpace(p.IdentityUserIdDecision) ? "N/D" : p.IdentityUserIdDecision!,
                    Cantidad = dias,
                    UnidadCantidad = "dias",
                    Monto = Redondear(salarioDiario * dias)
                };
            })
            .Where(x => x.Cantidad.GetValueOrDefault() > 0m)
            .ToList();
        if (detallesPermisoSinGoce.Count > 0)
        {
            detallesPorToken[TokenPermisoSinGoce] = detallesPermisoSinGoce;
            cantidadPorToken[TokenPermisoSinGoce] = detallesPermisoSinGoce.Sum(x => x.Cantidad ?? 0m);
            unidadPorToken[TokenPermisoSinGoce] = "dias";
        }

        var detallesIncapacidad = incapacidadesPeriodo
            .Where(x => x.IdEstado.HasValue && estadosIncapacidadValidos.Contains(x.IdEstado.Value))
            .Select(i =>
            {
                var fechas = ObtenerFechasRangoSolapado(periodoInicio.Date, periodoFin.Date, i.FechaInicio!.Value.Date, i.FechaFin!.Value.Date);
                var dias = fechas.Count;
                return new NominaConceptoDetalleDTO
                {
                    Fecha = i.FechaInicio?.Date,
                    Motivo = string.IsNullOrWhiteSpace(i.ComentarioSolicitud)
                        ? $"Incapacidad ({i.FechaInicio:yyyy-MM-dd} a {i.FechaFin:yyyy-MM-dd})"
                        : i.ComentarioSolicitud!,
                    GestionadoPor = string.IsNullOrWhiteSpace(i.IdentityUserIdDecision) ? "N/D" : i.IdentityUserIdDecision!,
                    Cantidad = dias,
                    UnidadCantidad = "dias",
                    Monto = Redondear(salarioDiario * dias)
                };
            })
            .Where(x => x.Cantidad.GetValueOrDefault() > 0m)
            .ToList();
        if (detallesIncapacidad.Count > 0)
        {
            detallesPorToken[TokenIncapacidad] = detallesIncapacidad;
            cantidadPorToken[TokenIncapacidad] = detallesIncapacidad.Sum(x => x.Cantidad ?? 0m);
            unidadPorToken[TokenIncapacidad] = "dias";
        }

        var tokensConcepto = conceptos
            .Select(x => ObtenerTokenConcepto(x))
            .ToHashSet();

        // Compatibilidad: si aun no existen conceptos separados, consolida en AUSENCIA.
        if (!tokensConcepto.Contains(TokenPermisoSinGoce) &&
            !tokensConcepto.Contains(TokenIncapacidad) &&
            tokensConcepto.Contains(TokenAusencia))
            descuentoAusencia = Redondear(descuentoAusencia + descuentoPermisoSinGoce + descuentoIncapacidad);

        var asignaciones = await _context.EmpleadosConceptoNomina
            .AsNoTracking()
            .Where(x =>
                x.Activo &&
                x.IdEmpleado == idEmpleado &&
                (!x.VigenciaDesde.HasValue || x.VigenciaDesde.Value.Date <= periodoFin.Date) &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value.Date >= periodoInicio.Date))
            .OrderBy(x => x.Prioridad)
            .ToListAsync();

        var asignacionPorConcepto = asignaciones
            .GroupBy(x => x.IdConceptoNomina)
            .ToDictionary(g => g.Key, g => g.First());

        var conceptosAplicados = new List<NominaConceptoAplicadoDTO>();
        var tokenEspecialIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var conceptoIndexPorToken = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var conceptoPorToken = conceptos
            .GroupBy(ObtenerTokenConcepto, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var baseCalculo = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["SB"] = salarioBase,
            ["HE"] = montoHoraExtra,
            ["AUSENCIA"] = descuentoAusencia,
            ["PERMISO_SIN_GOCE"] = descuentoPermisoSinGoce,
            ["INCAPACIDAD"] = descuentoIncapacidad,
            ["BR"] = salarioBase,
            ["BASE_PORCENTUAL"] = salarioBase,
            ["BASE_CCSS"] = salarioBase,
            ["BASE_RENTA"] = salarioBase
        };

        decimal baseCcssIngresos = 0m;
        decimal baseCcssDeducciones = 0m;
        decimal totalIngresosNoCcss = 0m;
        decimal totalDeduccionesNoCcss = 0m;
        decimal salarioBruto = 0m;
        decimal salarioNeto = 0m;

        foreach (var concepto in conceptos)
        {
            var token = ObtenerTokenConcepto(concepto);
            var tieneAsignacion = asignacionPorConcepto.TryGetValue(concepto.IdConceptoNomina, out var asignacion);
            var porcentajeAplicado = tieneAsignacion
                ? asignacion?.Porcentaje
                : concepto.ValorPorcentaje;

            var monto = tieneAsignacion
                ? CalcularMontoAsignacion(asignacion!, concepto, baseCalculo, salarioBase)
                : CalcularMontoConcepto(concepto, baseCalculo);

            switch (token)
            {
                case TokenSb:
                    monto = salarioBase;
                    baseCalculo["SB"] = monto;
                    break;
                case TokenHoraExtra:
                    monto = montoHoraExtra;
                    baseCalculo["HE"] = monto;
                    break;
                case TokenAusencia:
                    monto = descuentoAusencia;
                    baseCalculo["AUSENCIA"] = monto;
                    break;
                case TokenPermisoSinGoce:
                    monto = descuentoPermisoSinGoce;
                    baseCalculo["PERMISO_SIN_GOCE"] = monto;
                    break;
                case TokenIncapacidad:
                    monto = descuentoIncapacidad;
                    baseCalculo["INCAPACIDAD"] = monto;
                    break;
                case TokenTi:
                    var brutoParaIngresos = baseCalculo.TryGetValue("BR", out var brTi)
                        ? brTi
                        : Redondear(Math.Max(0m, baseCcssIngresos - baseCcssDeducciones));
                    monto = Redondear(brutoParaIngresos + totalIngresosNoCcss);
                    baseCalculo["TI"] = monto;
                    break;
                case TokenBr:
                    salarioBruto = Redondear(Math.Max(0m, baseCcssIngresos - baseCcssDeducciones));
                    monto = salarioBruto;
                    baseCalculo["BR"] = monto;
                    baseCalculo["BASE_PORCENTUAL"] = monto;
                    baseCalculo["BASE_CCSS"] = monto;
                    baseCalculo["BASE_RENTA"] = monto;
                    break;
                case TokenCcssSem:
                case TokenCcssIvm:
                case TokenCcssBp:
                    var baseCcss = baseCalculo.TryGetValue("BASE_CCSS", out var bCcss)
                        ? bCcss
                        : Redondear(Math.Max(0m, baseCcssIngresos - baseCcssDeducciones));
                    monto = Redondear(baseCcss * (concepto.ValorPorcentaje ?? 0m));
                    baseCalculo[token] = monto;
                    Trace($"[FORMULA {token}] baseCCSS={baseCcss:N2} * porcentaje={(concepto.ValorPorcentaje ?? 0m):0.######} => {monto:N2}");
                    break;
                case TokenRenta:
                    var baseRenta = baseCalculo.TryGetValue("BASE_RENTA", out var bRenta)
                        ? bRenta
                        : Redondear(Math.Max(0m, baseCcssIngresos - baseCcssDeducciones));
                    monto = Redondear(CalcularRentaPorTramos(baseRenta, tramosRentaVigentes));
                    baseCalculo[TokenRenta] = monto;
                    Trace($"[FORMULA RENTA] baseRenta={baseRenta:N2} | tramosVigentes={tramosRentaVigentes.Count} => {monto:N2}");
                    break;
                case TokenTd:
                    monto = Redondear(totalDeduccionesNoCcss);
                    baseCalculo["TD"] = monto;
                    break;
                case TokenNeto:
                    salarioBruto = baseCalculo.TryGetValue("BR", out var brutoCache)
                        ? brutoCache
                        : Redondear(Math.Max(0m, baseCcssIngresos - baseCcssDeducciones));
                    salarioNeto = Redondear(Math.Max(0m, salarioBruto + totalIngresosNoCcss - totalDeduccionesNoCcss));
                    monto = salarioNeto;
                    baseCalculo["NETO"] = monto;
                    break;
            }

            monto = Redondear(Math.Max(0m, monto));

            var afectaAcumulados = !EsConceptoTecnico(token);
            if (afectaAcumulados)
            {
                if (tipoPlanilla.AportaBaseCcss && EsDeduccionBaseCcss(token, concepto))
                    baseCcssDeducciones = Redondear(baseCcssDeducciones + monto);
                else if (tipoPlanilla.AportaBaseCcss && EsIngresoBaseCcss(token, concepto))
                    baseCcssIngresos = Redondear(baseCcssIngresos + monto);
                else if (EsDeduccionNoCcss(token, concepto))
                    totalDeduccionesNoCcss = Redondear(totalDeduccionesNoCcss + monto);
                else if (EsIngresoNoCcss(token, concepto))
                    totalIngresosNoCcss = Redondear(totalIngresosNoCcss + monto);
            }

            if (monto > 0m || EsConceptoTecnico(token) || EsConceptoRecalculable(token) || string.Equals(token, TokenSb, StringComparison.OrdinalIgnoreCase))
            {
                Trace($"[CONCEPTO] token={token} | codigo={concepto.CodigoConcepto ?? string.Empty} | nombre={concepto.Nombre} | modo={concepto.ModoCalculo?.Nombre ?? string.Empty} | ingreso={EsIngresoVisual(token, concepto)} | deduccion={EsDeduccionVisual(token, concepto)} | afectaBaseCCSS={AfectaBaseCcssVisual(token, concepto)} | porcentaje={porcentajeAplicado?.ToString("0.######") ?? "n/a"} | monto={monto:N2}");

                conceptosAplicados.Add(new NominaConceptoAplicadoDTO
                {
                    IdConceptoNomina = concepto.IdConceptoNomina,
                    CodigoConcepto = concepto.CodigoConcepto ?? string.Empty,
                    NombreConcepto = concepto.Nombre,
                    PorcentajeAplicado = porcentajeAplicado,
                    Monto = monto,
                    EsIngreso = EsIngresoVisual(token, concepto),
                    EsDeduccion = EsDeduccionVisual(token, concepto),
                    AfectaCcss = AfectaBaseCcssVisual(token, concepto),
                    EsSalarioBruto = string.Equals(token, TokenBr, StringComparison.Ordinal),
                    EsAjusteNoLaborado = EsNoLaboradoToken(token),
                    Cantidad = cantidadPorToken.TryGetValue(token, out var cantidad) ? cantidad : null,
                    UnidadCantidad = unidadPorToken.TryGetValue(token, out var unidad) ? unidad : null,
                    Detalles = detallesPorToken.TryGetValue(token, out var detalles)
                        ? detalles
                        : []
                });

                if (EsConceptoTecnico(token))
                    tokenEspecialIndex[token] = conceptosAplicados.Count - 1;

                if (!conceptoIndexPorToken.ContainsKey(token))
                    conceptoIndexPorToken[token] = conceptosAplicados.Count - 1;
            }
        }

        salarioBruto = Redondear(Math.Max(0m, baseCcssIngresos - baseCcssDeducciones));
        totalIngresosNoCcss = Redondear(totalIngresosNoCcss);
        totalDeduccionesNoCcss = Redondear(totalDeduccionesNoCcss);
        baseCalculo["BR"] = salarioBruto;
        baseCalculo["BASE_PORCENTUAL"] = salarioBruto;
        baseCalculo["BASE_CCSS"] = salarioBruto;
        baseCalculo["BASE_RENTA"] = salarioBruto;

        baseCalculo["TD"] = totalDeduccionesNoCcss;
        var totalIngresosConBruto = Redondear(salarioBruto + totalIngresosNoCcss);
        baseCalculo["TI"] = totalIngresosConBruto;

        if (conceptoPorToken.TryGetValue(TokenCcssSem, out var conceptoSem))
        {
            var montoSem = Redondear(salarioBruto * (conceptoSem.ValorPorcentaje ?? 0m));
            ActualizarConceptoRecalculable(TokenCcssSem, montoSem, conceptoSem);
        }

        if (conceptoPorToken.TryGetValue(TokenCcssIvm, out var conceptoIvm))
        {
            var montoIvm = Redondear(salarioBruto * (conceptoIvm.ValorPorcentaje ?? 0m));
            ActualizarConceptoRecalculable(TokenCcssIvm, montoIvm, conceptoIvm);
        }

        if (conceptoPorToken.TryGetValue(TokenCcssBp, out var conceptoBp))
        {
            var montoBp = Redondear(salarioBruto * (conceptoBp.ValorPorcentaje ?? 0m));
            ActualizarConceptoRecalculable(TokenCcssBp, montoBp, conceptoBp);
        }

        if (conceptoPorToken.TryGetValue(TokenRenta, out var conceptoRenta))
        {
            var inicioMesNomina = ObtenerMesNomina(periodoFin.Date).InicioMes;
            var baseRentaPeriodo = CalcularBaseRentaPeriodoActual(conceptosAplicados, conceptos, tipoPlanilla.AportaBaseRentaMensual);
            var (baseRentaAcumuladaPrev, rentaRetenidaPrevia) = await ObtenerAcumuladosMesPreviosAsync(
                idPlanilla,
                idEmpleado,
                inicioMesNomina,
                periodoInicio.Date,
                tipoPlanilla.AportaBaseRentaMensual);

            var baseRentaMensual = tipoPlanilla.AportaBaseRentaMensual
                ? Redondear(Math.Max(0m, baseRentaAcumuladaPrev + baseRentaPeriodo))
                : 0m;

            var rentaMensualAcumulada = tipoPlanilla.AportaBaseRentaMensual
                ? Redondear(CalcularRentaPorTramos(baseRentaMensual, tramosRentaVigentes))
                : 0m;

            var montoRenta = Redondear(Math.Max(0m, rentaMensualAcumulada - rentaRetenidaPrevia));

            Trace($"[RENTA-MENSUAL] mes={inicioMesNomina:yyyy-MM} | basePrev={baseRentaAcumuladaPrev:N2} | basePeriodo={baseRentaPeriodo:N2} | baseMensual={baseRentaMensual:N2} | rentaAcumulada={rentaMensualAcumulada:N2} | retenidoPrevio={rentaRetenidaPrevia:N2} | rentaPeriodo={montoRenta:N2}");
            ActualizarConceptoRecalculable(TokenRenta, montoRenta, conceptoRenta);
        }

        totalIngresosNoCcss = Redondear(conceptosAplicados
            .Where(c => !EsConceptoTecnicoCodigo(c.CodigoConcepto) && c.EsIngreso && !c.AfectaCcss && !c.EsAjusteNoLaborado)
            .Sum(c => c.Monto));

        totalDeduccionesNoCcss = Redondear(conceptosAplicados
            .Where(c => !EsConceptoTecnicoCodigo(c.CodigoConcepto) && c.EsDeduccion && !c.EsAjusteNoLaborado)
            .Sum(c => c.Monto));

        totalIngresosConBruto = Redondear(salarioBruto + totalIngresosNoCcss);
        baseCalculo["TI"] = totalIngresosConBruto;
        baseCalculo["TD"] = totalDeduccionesNoCcss;

        salarioNeto = Redondear(Math.Max(0m, salarioBruto + totalIngresosNoCcss - totalDeduccionesNoCcss));
        baseCalculo["NETO"] = salarioNeto;

        if (tokenEspecialIndex.TryGetValue(TokenBr, out var idxBr))
            conceptosAplicados[idxBr].Monto = salarioBruto;
        if (tokenEspecialIndex.TryGetValue(TokenTi, out var idxTi))
            conceptosAplicados[idxTi].Monto = totalIngresosConBruto;
        if (tokenEspecialIndex.TryGetValue(TokenTd, out var idxTd))
            conceptosAplicados[idxTd].Monto = totalDeduccionesNoCcss;
        if (tokenEspecialIndex.TryGetValue(TokenNeto, out var idxNeto))
            conceptosAplicados[idxNeto].Monto = salarioNeto;

        var salarioBaseAplicado = Redondear(
            conceptosAplicados
                .Where(c => string.Equals(c.CodigoConcepto, TokenSb, StringComparison.OrdinalIgnoreCase))
                .Sum(c => c.Monto));

        Trace($"[TOTALES] SB={salarioBase:N2} | BR={salarioBruto:N2} | IngresosNoCCSS={totalIngresosNoCcss:N2} | TotalIngresos={totalIngresosConBruto:N2} | DeduccionesNoCCSS={totalDeduccionesNoCcss:N2} | NETO={salarioNeto:N2}");

        if (trazaDetallada)
        {
            var bloque = string.Join(Environment.NewLine, new[]
            {
                "",
                "==================== NOMINA TRACE ====================",
                $"Empleado: {empleado.IdEmpleado} - {empleado.NombreCompleto}",
                $"Planilla: {idPlanilla}",
                $"Periodo : {periodoInicio:yyyy-MM-dd} .. {periodoFin:yyyy-MM-dd}",
                "------------------------------------------------------",
                string.Join(Environment.NewLine, traceLines!),
                "======================================================"
            });

            _logger.LogInformation("{Bloque}", bloque);
        }

        return new NominaCalculoEmpleadoDTO
        {
            IdEmpleado = empleado.IdEmpleado,
            NombreEmpleado = empleado.NombreCompleto,
            SalarioBase = salarioBaseAplicado,
            TotalIngresos = totalIngresosConBruto,
            SalarioBruto = salarioBruto,
            TotalDeducciones = totalDeduccionesNoCcss,
            SalarioNeto = salarioNeto,
            Conceptos = conceptosAplicados
        };

        void ActualizarConceptoRecalculable(string token, decimal monto, TipoConceptoNomina concepto)
        {
            if (!conceptoIndexPorToken.TryGetValue(token, out var idx))
                return;

            var nuevoMonto = Redondear(Math.Max(0m, monto));
            conceptosAplicados[idx].Monto = nuevoMonto;
            conceptosAplicados[idx].EsIngreso = EsIngresoVisual(token, concepto);
            conceptosAplicados[idx].EsDeduccion = EsDeduccionVisual(token, concepto);
            conceptosAplicados[idx].AfectaCcss = AfectaBaseCcssVisual(token, concepto);
        }
    }

    private static bool EsConceptoTecnico(string token) =>
        token is TokenTi or TokenBr or TokenTd or TokenNeto;

    private static bool EsConceptoTecnicoCodigo(string? codigo)
    {
        var token = NormalizarToken(codigo);
        return token is TokenTi or TokenTd or TokenNeto;
    }

    private static bool EsConceptoRecalculable(string token) =>
        token is TokenCcssSem or TokenCcssIvm or TokenCcssBp or TokenRenta;

    private static bool EsConceptoAutomaticoPlanillaRegular(string token) =>
        token is TokenSb or TokenHoraExtra or TokenAusencia or TokenPermisoSinGoce or TokenIncapacidad
            or TokenCcssSem or TokenCcssIvm or TokenCcssBp or TokenRenta;

    private static (DateTime InicioMes, DateTime FinMes) ObtenerMesNomina(DateTime fechaReferencia)
    {
        var inicio = new DateTime(fechaReferencia.Year, fechaReferencia.Month, 1);
        return (inicio, inicio.AddMonths(1).AddDays(-1));
    }

    private static bool EsIngresoBaseCcss(string token, TipoConceptoNomina concepto) =>
        token == TokenSb || (concepto.EsIngreso && concepto.AfectaCcss);

    private static bool EsDeduccionBaseCcss(string token, TipoConceptoNomina concepto) =>
        EsNoLaboradoToken(token);

    private static bool EsIngresoNoCcss(string token, TipoConceptoNomina concepto) =>
        !EsConceptoTecnico(token) && !EsNoLaboradoToken(token) && concepto.EsIngreso && !concepto.AfectaCcss;

    private static bool EsDeduccionNoCcss(string token, TipoConceptoNomina concepto) =>
        !EsConceptoTecnico(token) && !EsNoLaboradoToken(token) && concepto.EsDeduccion;

    private static bool EsIngresoVisual(string token, TipoConceptoNomina concepto) =>
        token == TokenBr || EsIngresoBaseCcss(token, concepto) || EsIngresoNoCcss(token, concepto);

    private static bool EsDeduccionVisual(string token, TipoConceptoNomina concepto) =>
        EsDeduccionBaseCcss(token, concepto) || EsDeduccionNoCcss(token, concepto);

    private static bool AfectaBaseCcssVisual(string token, TipoConceptoNomina concepto) =>
        token == TokenBr || EsIngresoBaseCcss(token, concepto) || EsDeduccionBaseCcss(token, concepto);

    private static decimal CalcularMontoConcepto(TipoConceptoNomina concepto, IDictionary<string, decimal> baseCalculo)
    {
        var modo = (concepto.ModoCalculo?.Nombre ?? string.Empty).Trim().ToUpperInvariant();
        var token = ObtenerTokenConcepto(concepto);

        if (token is TokenSb or TokenHoraExtra or TokenAusencia or TokenPermisoSinGoce or TokenIncapacidad
            or TokenTi or TokenBr or TokenTd or TokenNeto or TokenCcssSem or TokenCcssIvm or TokenCcssBp or TokenRenta)
            return 0m;

        return modo switch
        {
            "FIJO" => concepto.ValorFijo ?? 0m,
            "PORCENTAJE" => CalcularPorcentaje(concepto, baseCalculo),
            "FORMULA" => CalcularFormulaSimple(concepto, baseCalculo),
            "ACUMULADOR" => concepto.EsIngreso ? ObtenerValor(baseCalculo, "TI") : ObtenerValor(baseCalculo, "TD"),
            _ => 0m
        };
    }

    private static decimal CalcularPorcentaje(TipoConceptoNomina concepto, IDictionary<string, decimal> baseCalculo)
    {
        var porcentaje = concepto.ValorPorcentaje ?? 0m;
        if (porcentaje <= 0m) return 0m;

        var baseCodigo = concepto.EsDeduccion ? "BR" : DeterminarBasePorcentaje((concepto.CodigoFormula ?? string.Empty).Trim().ToUpperInvariant(), false);
        var baseMonto = ObtenerValor(baseCalculo, baseCodigo);
        return baseMonto * porcentaje;
    }

    private static decimal CalcularFormulaSimple(TipoConceptoNomina concepto, IDictionary<string, decimal> baseCalculo)
    {
        var codigoFormula = (concepto.CodigoFormula ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(codigoFormula)) return 0m;
        return ObtenerValor(baseCalculo, codigoFormula);
    }

    private static decimal CalcularMontoAsignacion(
        EmpleadoConceptoNomina asignacion,
        TipoConceptoNomina concepto,
        IDictionary<string, decimal> baseCalculo,
        decimal salarioBase)
    {
        var monto = 0m;

        if (asignacion.MontoFijo.HasValue)
            monto = asignacion.MontoFijo.Value;
        else if (asignacion.Porcentaje.HasValue)
        {
            var codigoFormula = (concepto.CodigoFormula ?? string.Empty).Trim().ToUpperInvariant();
            var baseCodigo = concepto.EsDeduccion ? "BR" : DeterminarBasePorcentaje(codigoFormula, false);

            decimal baseMonto = baseCodigo switch
            {
                "SB" => salarioBase,
                "BR" => (baseCalculo.TryGetValue("BR", out var br) ? br : salarioBase),
                _ => ObtenerValor(baseCalculo, baseCodigo)
            };

            monto = baseMonto * asignacion.Porcentaje.Value;
        }

        if (asignacion.SaldoPendiente.HasValue && concepto.EsDeduccion)
            monto = Math.Min(monto, asignacion.SaldoPendiente.Value);

        return monto;
    }

    private static string ObtenerTokenConcepto(TipoConceptoNomina concepto)
        => NormalizarToken(!string.IsNullOrWhiteSpace(concepto.CodigoFormula)
            ? concepto.CodigoFormula
            : concepto.CodigoConcepto);

    private static string NormalizarToken(string? token) =>
        (token ?? string.Empty).Trim().ToUpperInvariant();

    private static decimal Redondear(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    private static decimal ObtenerValor(IDictionary<string, decimal> valores, string llave) =>
        valores.TryGetValue(llave, out var valor) ? valor : 0m;

    private static string DeterminarBasePorcentaje(string codigoFormula, bool esDeduccion)
    {
        if (codigoFormula.Contains("BASE_PORCENTUAL", StringComparison.OrdinalIgnoreCase))
            return "BASE_PORCENTUAL";
        if (codigoFormula.Contains("BASE_CCSS", StringComparison.OrdinalIgnoreCase))
            return "BASE_CCSS";
        if (codigoFormula.Contains("BASE_RENTA", StringComparison.OrdinalIgnoreCase))
            return "BASE_RENTA";
        if (codigoFormula.Contains("SB", StringComparison.OrdinalIgnoreCase))
            return "SB";
        if (codigoFormula.Contains("BR", StringComparison.OrdinalIgnoreCase))
            return "BR";

        return esDeduccion ? "BASE_PORCENTUAL" : "BR";
    }

    private static bool EsNoLaboradoToken(string token) =>
        token is TokenAusencia or TokenPermisoSinGoce or TokenIncapacidad;

    private static bool EsTokenResumenORenta(string token) =>
        token is TokenTi or TokenBr or TokenTd or TokenNeto or TokenRenta;

    private static bool ConceptoAfectaBaseRenta(TipoConceptoNomina concepto, bool tipoPlanillaAportaBaseRenta)
    {
        if (!tipoPlanillaAportaBaseRenta)
            return false;

        var token = ObtenerTokenConcepto(concepto);
        if (EsTokenResumenORenta(token))
            return false;

        return concepto.AfectaRenta;
    }

    private static decimal CalcularBaseRentaPeriodoActual(
        IReadOnlyCollection<NominaConceptoAplicadoDTO> conceptosAplicados,
        IReadOnlyCollection<TipoConceptoNomina> conceptosDefinidos,
        bool tipoPlanillaAportaBaseRenta)
    {
        var conceptoPorId = conceptosDefinidos.ToDictionary(c => c.IdConceptoNomina);
        decimal totalIngresos = 0m;
        decimal totalDeducciones = 0m;

        foreach (var aplicado in conceptosAplicados)
        {
            if (!conceptoPorId.TryGetValue(aplicado.IdConceptoNomina, out var definicion))
                continue;

            if (!ConceptoAfectaBaseRenta(definicion, tipoPlanillaAportaBaseRenta))
                continue;

            if (aplicado.EsIngreso)
                totalIngresos = Redondear(totalIngresos + aplicado.Monto);
            else if (aplicado.EsDeduccion)
                totalDeducciones = Redondear(totalDeducciones + aplicado.Monto);
        }

        return Redondear(Math.Max(0m, totalIngresos - totalDeducciones));
    }

    private async Task<(decimal BaseRentaAcumulada, decimal RentaRetenida)> ObtenerAcumuladosMesPreviosAsync(
        int idPlanillaActual,
        int idEmpleado,
        DateTime inicioMes,
        DateTime inicioPeriodoActual,
        bool tipoPlanillaAportaBaseRenta)
    {
        var conceptosPrevios = await _context.PlanillasDetalleConcepto
            .AsNoTracking()
            .Where(c =>
                c.PlanillaDetalle != null &&
                c.PlanillaDetalle.IdEmpleado == idEmpleado &&
                c.PlanillaDetalle.IdPlanilla != idPlanillaActual &&
                c.PlanillaDetalle.Planilla != null &&
                c.PlanillaDetalle.Planilla.PeriodoInicio.Date >= inicioMes.Date &&
                c.PlanillaDetalle.Planilla.PeriodoInicio.Date < inicioPeriodoActual.Date)
            .Include(c => c.TipoConceptoNomina)
            .ToListAsync();

        decimal baseRenta = 0m;
        decimal rentaRetenida = 0m;

        foreach (var concepto in conceptosPrevios)
        {
            if (concepto.TipoConceptoNomina is null)
                continue;

            var token = ObtenerTokenConcepto(concepto.TipoConceptoNomina);
            if (string.Equals(token, TokenRenta, StringComparison.OrdinalIgnoreCase))
            {
                rentaRetenida = Redondear(rentaRetenida + concepto.Monto);
                continue;
            }

            if (!ConceptoAfectaBaseRenta(concepto.TipoConceptoNomina, tipoPlanillaAportaBaseRenta))
                continue;

            if (concepto.TipoConceptoNomina.EsIngreso)
                baseRenta = Redondear(baseRenta + concepto.Monto);
            else if (concepto.TipoConceptoNomina.EsDeduccion)
                baseRenta = Redondear(baseRenta - concepto.Monto);
        }

        return (Redondear(Math.Max(0m, baseRenta)), rentaRetenida);
    }

    private static decimal CalcularRentaPorTramos(decimal baseRentaMensual, IReadOnlyCollection<TramoRentaSalario> tramos)
    {
        if (baseRentaMensual <= 0m) return 0m;
        if (tramos.Count == 0)
            throw new BusinessException("No hay tramos de renta vigentes configurados para el periodo.");

        decimal impuesto = 0m;
        foreach (var tramo in tramos)
            impuesto += Tramo(baseRentaMensual, tramo.DesdeMonto, tramo.HastaMonto, tramo.Tasa);

        return impuesto;
    }

    private static decimal Tramo(decimal baseRenta, decimal desde, decimal? hasta, decimal tasa)
    {
        if (baseRenta <= desde) return 0m;
        var limite = hasta ?? baseRenta;
        var baseTramo = Math.Min(baseRenta, limite) - desde;
        return baseTramo <= 0m ? 0m : baseTramo * tasa;
    }

    private static DateTime Max(DateTime a, DateTime b) => a >= b ? a : b;
    private static DateTime Min(DateTime a, DateTime b) => a <= b ? a : b;

    private static IReadOnlyCollection<DateTime> ObtenerFechasRangoSolapado(DateTime inicioPeriodo, DateTime finPeriodo, DateTime inicioSolicitud, DateTime finSolicitud)
    {
        var inicio = Max(inicioPeriodo, inicioSolicitud);
        var fin = Min(finPeriodo, finSolicitud);
        if (fin < inicio)
            return Array.Empty<DateTime>();

        var fechas = new List<DateTime>();
        var cursor = inicio;
        while (cursor <= fin)
        {
            fechas.Add(cursor);
            cursor = cursor.AddDays(1);
        }

        return fechas;
    }
}
