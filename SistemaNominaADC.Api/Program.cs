using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaNominaADC.Api;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Api.Reports;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;
using SistemaNominaADC.Negocio.Servicios;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1) DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var sIssuer = jwtSettings["Issuer"];
var sAudience = jwtSettings["Audience"];
var sKey = jwtSettings["Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = sIssuer,
        ValidAudience = sAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sKey!)),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddScoped<IEstadoService, EstadoService>();
builder.Services.AddScoped<IObjetoSistemaService, ObjetoSistemaService>();
builder.Services.AddScoped<IObjetoSistemaAuthorizationService, ObjetoSistemaAuthorizationService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISolicitudesAuthorizationService, SolicitudesAuthorizationService>();
builder.Services.AddScoped<IGrupoEstadoService, GrupoEstadoService>();
builder.Services.AddScoped<IDepartamentoService, DepartamentoService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPuestoService, PuestoService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IEmpleadoConceptoNominaService, EmpleadoConceptoNominaService>();
builder.Services.AddScoped<ITipoPermisoService, TipoPermisoService>();
builder.Services.AddScoped<ITipoIncapacidadService, TipoIncapacidadService>();
builder.Services.AddScoped<ITipoHoraExtraService, TipoHoraExtraService>();
builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();
builder.Services.AddScoped<IPermisoService, PermisoService>();
builder.Services.AddScoped<ISolicitudVacacionesService, SolicitudVacacionesService>();
builder.Services.AddScoped<ISolicitudHorasExtraService, SolicitudHorasExtraService>();
builder.Services.AddScoped<IIncapacidadService, IncapacidadService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<IDepartamentoJefaturaService, DepartamentoJefaturaService>();
builder.Services.AddScoped<IEmpleadoJerarquiaService, EmpleadoJerarquiaService>();
builder.Services.AddScoped<IModoCalculoConceptoNominaService, ModoCalculoConceptoNominaService>();
builder.Services.AddScoped<ITipoConceptoNominaService, TipoConceptoNominaService>();
builder.Services.AddScoped<ITipoPlanillaService, TipoPlanillaService>();
builder.Services.AddScoped<ITipoPlanillaConceptoService, TipoPlanillaConceptoService>();
builder.Services.AddScoped<IFlujoEstadoService, FlujoEstadoService>();
builder.Services.AddScoped<IFlujoEstadoMantenimientoService, FlujoEstadoMantenimientoService>();
builder.Services.AddScoped<IPlanillaEncabezadoService, PlanillaEncabezadoService>();
builder.Services.AddScoped<INominaCalculator, NominaCalculator>();
builder.Services.AddScoped<IComprobantePlanillaService, ComprobantePlanillaService>();
builder.Services.AddScoped<INominaService, NominaService>();
builder.Services.AddScoped<IMiPlanillaService, MiPlanillaService>();
builder.Services.AddScoped<ITramoRentaSalarioService, TramoRentaSalarioService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// 6) CORS (mejor limitar origen de tu Blazor; ajusta puertos si cambian)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy
            .WithOrigins("https://localhost:7252", "http://localhost:5166")
            .AllowAnyHeader()
            .AllowAnyMethod();
        // No AllowCredentials porque JWT va en header Authorization (no cookies).
    });
});

var app = builder.Build();

await SeedObjetosSistemaAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazor");

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

static async Task SeedObjetosSistemaAsync(IServiceProvider services)
{
    try
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var idGrupo = await context.GrupoEstados
            .AsNoTracking()
            .Where(g => g.Activo)
            .OrderBy(g => g.IdGrupoEstado)
            .Select(g => g.IdGrupoEstado)
            .FirstOrDefaultAsync();

        if (idGrupo <= 0)
        {
            var grupoDefault = new GrupoEstado
            {
                Nombre = "GENERAL",
                Descripcion = "Grupo creado automaticamente para objetos del sistema.",
                Activo = true
            };

            context.GrupoEstados.Add(grupoDefault);
            await context.SaveChangesAsync();
            idGrupo = grupoDefault.IdGrupoEstado;
        }

        var existentes = await context.ObjetoSistemas
            .AsNoTracking()
            .Select(o => o.NombreEntidad)
            .ToListAsync();

        var existentesSet = new HashSet<string>(existentes, StringComparer.OrdinalIgnoreCase);

        var nuevos = ObjetoSistemaCatalogo.Items
            .Select(i => i.NombreEntidad)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(nombre => !existentesSet.Contains(nombre))
            .Select(nombre => new ObjetoSistema
            {
                NombreEntidad = nombre,
                IdGrupoEstado = idGrupo
            })
            .ToList();

        if (nuevos.Count == 0)
            return;

        context.ObjetoSistemas.AddRange(nuevos);
        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seed de objetos del sistema omitido: {ex.Message}");
    }
}
