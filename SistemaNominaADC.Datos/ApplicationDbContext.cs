using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // CONFIGURACIÓN DE DEPARTAMENTO
            builder.Entity<Departamento>(entity =>
            {
                entity.ToTable("Departamento"); 
                entity.HasKey(e => e.IdDepartamento);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IdEstado).HasColumnName("IdEstado").IsRequired();
                entity.HasMany<Puesto>()
                      .WithOne(p => p.Departamento)
                      .HasForeignKey(p => p.IdDepartamento)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Estado)
                      .WithMany()
                      .HasForeignKey(e => e.IdEstado);

            });

            // CONFIGURACIÓN DE PUESTO
            builder.Entity<Puesto>(entity =>
            {
                entity.ToTable("Puesto");
                entity.HasKey(e => e.IdPuesto);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Estado).IsRequired();
            });

            // CONFIGURACIÓN DE EMPLEADO
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
                entity.Property(e => e.SalarioBase).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Estado).IsRequired();
                entity.Property(e => e.FechaIngreso).IsRequired();
            });

            // CONFIGURACIÓN DE BITACORA 
            builder.Entity<Bitacora>(entity =>
            {
                entity.ToTable("Bitacora");
                entity.HasKey(e => e.IdBitacora);
                entity.Property(e => e.IdEmpleado).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Accion).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Detalle).IsRequired();

            });
            // CONFIGURACIÓN DE ESTADO 
            builder.Entity<Estado>(entity =>
            {
                entity.ToTable("Estado");
                entity.HasKey(e => e.IdEstado);

                entity.Property(e => e.EstadoActivo).HasColumnName("Estado");

                entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
            });
            // CONFIGURACIÓN DE GRUPO ESTADO Y DETALLE
            builder.Entity<GrupoEstadoDetalle>()
            .HasKey(cd => new { cd.IdGrupoEstado, cd.IdEstado });

            builder.Entity<GrupoEstado>().ToTable("GrupoEstado");
            builder.Entity<GrupoEstadoDetalle>().ToTable("GrupoEstadoDetalle");

            // CONFIGURACIÓN DE OBJETO SISTEMA
            builder.Entity<ObjetoSistema>().ToTable("ObjetoSistema");


        }
    }
}

