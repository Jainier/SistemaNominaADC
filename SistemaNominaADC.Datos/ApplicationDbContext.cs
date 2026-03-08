using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Datos
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Puesto> Puestos { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Bitacora> Bitacoras { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<GrupoEstado> GrupoEstados { get; set; }
        public DbSet<GrupoEstadoDetalle> GrupoEstadoDetalles { get; set; }
        public DbSet<ObjetoSistema> ObjetoSistemas { get; set; }
        public DbSet<ObjetoSistemaRol> ObjetoSistemaRoles { get; set; }
        public DbSet<TipoPermiso> TipoPermisos { get; set; }
        public DbSet<TipoIncapacidad> TipoIncapacidades { get; set; }
        public DbSet<TipoHoraExtra> TipoHoraExtras { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<SolicitudVacaciones> SolicitudesVacaciones { get; set; }
        public DbSet<SolicitudHorasExtra> SolicitudesHorasExtra { get; set; }
        public DbSet<Incapacidad> Incapacidades { get; set; }
        public DbSet<Vacaciones> Vacaciones { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<DepartamentoJefatura> DepartamentoJefaturas { get; set; }
        public DbSet<EmpleadoJerarquia> EmpleadoJerarquias { get; set; }
        public DbSet<ModoCalculoConceptoNomina> ModosCalculoConceptoNomina { get; set; }
        public DbSet<TipoConceptoNomina> TiposConceptoNomina { get; set; }
        public DbSet<TipoPlanilla> TiposPlanilla { get; set; }
        public DbSet<TipoPlanillaConcepto> TiposPlanillaConcepto { get; set; }
        public DbSet<PlanillaEncabezado> PlanillasEncabezado { get; set; }
        public DbSet<PlanillaDetalle> PlanillasDetalle { get; set; }
        public DbSet<PlanillaDetalleConcepto> PlanillasDetalleConcepto { get; set; }
        public DbSet<FlujoEstado> FlujosEstado { get; set; }
        public DbSet<EmpleadoConceptoNomina> EmpleadosConceptoNomina { get; set; }
        public DbSet<TramoRentaSalario> TramosRentaSalario { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Departamento>(entity =>
            {
                entity.ToTable("Departamento");
                entity.HasKey(e => e.IdDepartamento);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.IdEstado).HasColumnName("IdEstado").IsRequired();
                entity.HasMany<Puesto>()
                    .WithOne(p => p.Departamento)
                    .HasForeignKey(p => p.IdDepartamento)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado);
            });

            builder.Entity<Puesto>(entity =>
            {
                entity.ToTable("Puesto");
                entity.HasKey(e => e.IdPuesto);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.SalarioBase).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.IdEstado).IsRequired();
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado);
            });

            builder.Entity<Empleado>(entity =>
            {
                entity.ToTable("Empleado");
                entity.HasKey(e => e.IdEmpleado);
                entity.Property(e => e.Cedula).IsRequired().HasMaxLength(20);
                entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(200);
                entity.HasOne(e => e.Puesto)
                    .WithMany()
                    .HasForeignKey(e => e.IdPuesto)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.SalarioBase).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.IdEstado).IsRequired();
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado);
                entity.Property(e => e.FechaIngreso).IsRequired();
            });

            builder.Entity<Bitacora>(entity =>
            {
                entity.ToTable("Bitacora");
                entity.HasKey(e => e.IdBitacora);
                entity.Property(e => e.Accion).HasMaxLength(150).IsUnicode(false);
                entity.Property(e => e.Descripcion).HasColumnType("text");
                entity.Property(e => e.IdentityUserId).HasMaxLength(450);
            });

            builder.Entity<Estado>(entity =>
            {
                entity.ToTable("Estado");
                entity.HasKey(e => e.IdEstado);
                entity.Property(e => e.EstadoActivo).HasColumnName("Estado");
                entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
            });

            builder.Entity<GrupoEstadoDetalle>()
                .HasKey(cd => new { cd.IdGrupoEstado, cd.IdEstado });

            builder.Entity<GrupoEstado>(entity =>
            {
                entity.ToTable("GrupoEstado");
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Activo).HasDefaultValue(true);
            });
            builder.Entity<GrupoEstadoDetalle>().ToTable("GrupoEstadoDetalle");
            builder.Entity<ObjetoSistema>(entity =>
            {
                entity.ToTable("ObjetoSistema");
                entity.HasIndex(e => e.NombreEntidad).IsUnique();
            });
            builder.Entity<ObjetoSistemaRol>(entity =>
            {
                entity.ToTable("ObjetoSistemaRol");
                entity.HasKey(e => new { e.IdObjeto, e.RoleName });
                entity.Property(e => e.RoleName).HasMaxLength(256).IsRequired();
                entity.HasOne(e => e.ObjetoSistema)
                    .WithMany()
                    .HasForeignKey(e => e.IdObjeto)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TipoPermiso>(entity =>
            {
                entity.ToTable("TipoPermiso");
                entity.HasKey(e => e.IdTipoPermiso);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ConGoceSalarial).HasDefaultValue(true);
                entity.Property(e => e.IdEstado).IsRequired();
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TipoIncapacidad>(entity =>
            {
                entity.ToTable("TipoIncapacidad");
                entity.HasKey(e => e.IdTipoIncapacidad);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IdEstado).IsRequired();
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TipoHoraExtra>(entity =>
            {
                entity.ToTable("TipoHoraExtra");
                entity.HasKey(e => e.IdTipoHoraExtra);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PorcentajePago).HasColumnType("decimal(5,4)");
                entity.Property(e => e.IdEstado).IsRequired();
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Asistencia>(entity =>
            {
                entity.ToTable("Asistencia");
                entity.HasKey(e => e.IdAsistencia);
                entity.Property(e => e.Fecha).HasColumnType("date").IsRequired();
                entity.Property(e => e.HoraEntrada).HasColumnType("datetime");
                entity.Property(e => e.HoraSalida).HasColumnType("datetime");
                entity.Property(e => e.Ausencia);
                entity.Property(e => e.Justificacion).HasColumnType("text");
                entity.Property(e => e.IdEstado).IsRequired();
                entity.HasOne(e => e.Empleado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Permiso>(entity =>
            {
                entity.ToTable("Permiso");
                entity.HasKey(e => e.IdPermiso);
                entity.Property(e => e.FechaInicio).HasColumnType("date");
                entity.Property(e => e.FechaFin).HasColumnType("date");
                entity.Property(e => e.Motivo).HasMaxLength(200);
                entity.Property(e => e.ComentarioAprobacion).HasMaxLength(300);
                entity.Property(e => e.IdentityUserIdDecision).HasMaxLength(450);
                entity.HasOne(e => e.Empleado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.TipoPermiso)
                    .WithMany()
                    .HasForeignKey(e => e.IdTipoPermiso)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SolicitudVacaciones>(entity =>
            {
                entity.ToTable("SolicitudVacaciones");
                entity.HasKey(e => e.IdSolicitudVacaciones);
                entity.Property(e => e.FechaInicio).HasColumnType("date");
                entity.Property(e => e.FechaFin).HasColumnType("date");
                entity.Property(e => e.ComentarioSolicitud).HasMaxLength(300);
                entity.Property(e => e.ComentarioAprobacion).HasMaxLength(300);
                entity.Property(e => e.IdentityUserIdDecision).HasMaxLength(450);
                entity.HasOne(e => e.Empleado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SolicitudHorasExtra>(entity =>
            {
                entity.ToTable("SolicitudHorasExtra");
                entity.HasKey(e => e.IdSolicitudHorasExtra);
                entity.Property(e => e.Fecha).HasColumnType("date");
                entity.Property(e => e.CantidadHoras).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Motivo).HasMaxLength(200);
                entity.Property(e => e.ComentarioAprobacion).HasMaxLength(300);
                entity.Property(e => e.IdentityUserIdDecision).HasMaxLength(450);
                entity.HasOne(e => e.Empleado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.TipoHoraExtra)
                    .WithMany()
                    .HasForeignKey(e => e.IdTipoHoraExtra)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Incapacidad>(entity =>
            {
                entity.ToTable("Incapacidad");
                entity.HasKey(e => e.IdIncapacidad);
                entity.Property(e => e.FechaInicio).HasColumnType("date");
                entity.Property(e => e.FechaFin).HasColumnType("date");
                entity.Property(e => e.MontoCubierto).HasColumnType("decimal(10,2)");
                entity.Property(e => e.NombreDocumento).HasMaxLength(255);
                entity.Property(e => e.RutaDocumento).HasMaxLength(500);
                entity.Property(e => e.ComentarioRevision).HasMaxLength(300);
                entity.Property(e => e.ComentarioSolicitud).HasMaxLength(300);
                entity.Property(e => e.ComentarioAprobacion).HasMaxLength(300);
                entity.Property(e => e.IdentityUserIdDecision).HasMaxLength(450);

                entity.HasOne(e => e.Empleado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.TipoIncapacidad)
                    .WithMany()
                    .HasForeignKey(e => e.IdTipoIncapacidad)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Vacaciones>(entity =>
            {
                entity.ToTable("Vacaciones");
                entity.HasKey(e => e.IdVacaciones);
                entity.HasOne(e => e.Empleado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Notificacion>(entity =>
            {
                entity.ToTable("Notificacion");
                entity.HasKey(e => e.IdNotificacion);
                entity.Property(e => e.IdentityUserId).HasMaxLength(450).IsRequired();
                entity.Property(e => e.Titulo).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Mensaje).HasMaxLength(500).IsRequired();
                entity.Property(e => e.UrlDestino).HasMaxLength(300);
                entity.Property(e => e.Leida).HasDefaultValue(false);
                entity.Property(e => e.FechaCreacion).IsRequired();
            });

            builder.Entity<DepartamentoJefatura>(entity =>
            {
                entity.ToTable("DepartamentoJefatura");
                entity.HasKey(e => e.IdDepartamentoJefatura);
                entity.Property(e => e.TipoJefatura).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.VigenciaDesde).HasColumnType("date");
                entity.Property(e => e.VigenciaHasta).HasColumnType("date");

                entity.HasOne(e => e.Departamento)
                    .WithMany()
                    .HasForeignKey(e => e.IdDepartamento)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Empleado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<EmpleadoJerarquia>(entity =>
            {
                entity.ToTable("EmpleadoJerarquia");
                entity.HasKey(e => e.IdEmpleadoJerarquia);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.VigenciaDesde).HasColumnType("date");
                entity.Property(e => e.VigenciaHasta).HasColumnType("date");
                entity.Property(e => e.Observacion).HasMaxLength(250);

                entity.HasOne(e => e.Empleado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Supervisor)
                    .WithMany()
                    .HasForeignKey(e => e.IdSupervisor)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ModoCalculoConceptoNomina>(entity =>
            {
                entity.ToTable("ModoCalculoConceptoNomina");
                entity.HasKey(e => e.IdModoCalculoConceptoNomina);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100).IsUnicode(false);
                entity.Property(e => e.Descripcion).HasMaxLength(250).IsUnicode(false);
                entity.Property(e => e.IdEstado).IsRequired();
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TipoConceptoNomina>(entity =>
            {
                entity.ToTable("TipoConceptoNomina");
                entity.HasKey(e => e.IdConceptoNomina);
                entity.Property(e => e.CodigoConcepto).HasMaxLength(40).IsUnicode(false);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150).IsUnicode(false);
                entity.Property(e => e.FormulaCalculo).HasMaxLength(1000).IsUnicode(false);
                entity.Property(e => e.CodigoFormula).HasMaxLength(60).IsUnicode(false);
                entity.Property(e => e.ValorPorcentaje).HasColumnType("decimal(9,6)");
                entity.Property(e => e.ValorFijo).HasColumnType("decimal(10,2)");
                entity.Property(e => e.OrdenCalculo).HasDefaultValue(0);
                entity.Property(e => e.IdEstado).IsRequired();
                entity.Property(e => e.AfectaCcss).HasDefaultValue(true);
                entity.Property(e => e.AfectaRenta).HasDefaultValue(true);
                entity.HasOne(e => e.ModoCalculo)
                    .WithMany()
                    .HasForeignKey(e => e.IdModoCalculo)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.CodigoConcepto)
                    .IsUnique()
                    .HasFilter("[CodigoConcepto] IS NOT NULL");
            });

            builder.Entity<TipoPlanilla>(entity =>
            {
                entity.ToTable("TipoPlanilla");
                entity.HasKey(e => e.IdTipoPlanilla);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100).IsUnicode(false);
                entity.Property(e => e.Descripcion).HasMaxLength(100).IsUnicode(false);
                entity.Property(e => e.ModoCalculo).IsRequired().HasMaxLength(30).IsUnicode(false).HasDefaultValue("Regular");
                entity.Property(e => e.AportaBaseCcss).HasDefaultValue(true);
                entity.Property(e => e.AportaBaseRentaMensual).HasDefaultValue(true);
                entity.Property(e => e.IdEstado).IsRequired();
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TipoPlanillaConcepto>(entity =>
            {
                entity.ToTable("TipoPlanillaConcepto");
                entity.HasKey(e => new { e.IdTipoPlanilla, e.IdConceptoNomina });
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.Obligatorio).HasDefaultValue(false);
                entity.Property(e => e.PermiteMontoManual).HasDefaultValue(false);
                entity.Property(e => e.Prioridad).HasDefaultValue(0);

                entity.HasOne(e => e.TipoPlanilla)
                    .WithMany()
                    .HasForeignKey(e => e.IdTipoPlanilla)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TipoConceptoNomina)
                    .WithMany()
                    .HasForeignKey(e => e.IdConceptoNomina)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PlanillaEncabezado>(entity =>
            {
                entity.ToTable("PlanillaEncabezado");
                entity.HasKey(e => e.IdPlanilla);
                entity.Property(e => e.PeriodoInicio).HasColumnType("date").IsRequired();
                entity.Property(e => e.PeriodoFin).HasColumnType("date").IsRequired();
                entity.Property(e => e.FechaPago).HasColumnType("date").IsRequired();
                entity.Property(e => e.IdTipoPlanilla).IsRequired();
                entity.Property(e => e.IdEstado).IsRequired();
                entity.Property(e => e.IdentityUserIdDecision).HasMaxLength(450);
                entity.HasOne(e => e.TipoPlanilla)
                    .WithMany()
                    .HasForeignKey(e => e.IdTipoPlanilla)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PlanillaDetalle>(entity =>
            {
                entity.ToTable("PlanillaDetalle");
                entity.HasKey(e => e.IdPlanillaDetalle);
                entity.Property(e => e.SalarioBase).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TotalIngresos).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TotalDeducciones).HasColumnType("decimal(10,2)");
                entity.Property(e => e.SalarioBruto).HasColumnType("decimal(10,2)");
                entity.Property(e => e.SalarioNeto).HasColumnType("decimal(10,2)");
                entity.Property(e => e.NombreComprobantePdf).HasMaxLength(260);
                entity.Property(e => e.HashComprobantePdf).HasMaxLength(64);
                entity.Property(e => e.ComprobantePdf).HasColumnType("varbinary(max)");
                entity.Property(e => e.FechaGeneracionComprobantePdf).HasColumnType("datetime2");
                entity.Property(e => e.IdEstado).IsRequired();
                entity.HasOne(e => e.Planilla)
                    .WithMany()
                    .HasForeignKey(e => e.IdPlanilla)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Empleado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PlanillaDetalleConcepto>(entity =>
            {
                entity.ToTable("PlanillaDetalleConcepto");
                entity.HasKey(e => e.IdDetalleConcepto);
                entity.Property(e => e.Monto).HasColumnType("decimal(10,2)");
                entity.Property(e => e.IdEstado).IsRequired();
                entity.HasOne(e => e.PlanillaDetalle)
                    .WithMany()
                    .HasForeignKey(e => e.IdPlanillaDetalle)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.TipoConceptoNomina)
                    .WithMany()
                    .HasForeignKey(e => e.IdConceptoNomina)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<FlujoEstado>(entity =>
            {
                entity.ToTable("FlujoEstado");
                entity.HasKey(e => e.IdFlujoEstado);
                entity.Property(e => e.Entidad).IsRequired().HasMaxLength(100).IsUnicode(false);
                entity.Property(e => e.Accion).IsRequired().HasMaxLength(50).IsUnicode(false);
                entity.Property(e => e.RequiereRol).HasMaxLength(256).IsUnicode(false);
                entity.Property(e => e.Activo).HasDefaultValue(true);

                entity.HasOne(e => e.EstadoOrigen)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstadoOrigen)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EstadoDestino)
                    .WithMany()
                    .HasForeignKey(e => e.IdEstadoDestino)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.Entidad, e.Accion, e.IdEstadoOrigen, e.IdEstadoDestino })
                    .IsUnique();
            });

            builder.Entity<EmpleadoConceptoNomina>(entity =>
            {
                entity.ToTable("EmpleadoConceptoNomina");
                entity.HasKey(e => e.IdEmpleadoConceptoNomina);
                entity.Property(e => e.MontoFijo).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Porcentaje).HasColumnType("decimal(9,6)");
                entity.Property(e => e.SaldoPendiente).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Prioridad).HasDefaultValue(0);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.VigenciaDesde).HasColumnType("date");
                entity.Property(e => e.VigenciaHasta).HasColumnType("date");

                entity.HasOne(e => e.Empleado)
                    .WithMany()
                    .HasForeignKey(e => e.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TipoConceptoNomina)
                    .WithMany()
                    .HasForeignKey(e => e.IdConceptoNomina)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.IdEmpleado, e.IdConceptoNomina, e.VigenciaDesde, e.VigenciaHasta })
                    .IsUnique()
                    .HasFilter("[Activo] = 1");
            });

            builder.Entity<TramoRentaSalario>(entity =>
            {
                entity.ToTable("TramoRentaSalario");
                entity.HasKey(e => e.IdTramoRentaSalario);
                entity.Property(e => e.DesdeMonto).HasColumnType("decimal(18,2)");
                entity.Property(e => e.HastaMonto).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Tasa).HasColumnType("decimal(9,6)");
                entity.Property(e => e.VigenciaDesde).HasColumnType("date").IsRequired();
                entity.Property(e => e.VigenciaHasta).HasColumnType("date");
                entity.Property(e => e.Orden).HasDefaultValue(0);
                entity.Property(e => e.Activo).HasDefaultValue(true);
            });
        }
    }
}
