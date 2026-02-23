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
        }
    }
}
