using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Win32;
using SistemaNominaADC.Presentacion.Components;
using SistemaNominaADC.Presentacion.Security;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;
using SistemaNominaADC.Entidades;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


//Servicios API
/*builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7068/")
});*/

builder.Services.AddScoped<IRolCliente, RolCliente>();
builder.Services.AddScoped<IObjetoSistemaCliente, ObjetoSistemaCliente>();
builder.Services.AddScoped<IGrupoEstadoCliente, GrupoEstadoCliente>();
builder.Services.AddScoped<IEstadoCliente, EstadoCliente>();
builder.Services.AddScoped<IDepartamentoCliente, DepartamentoCliente>();
builder.Services.AddScoped<IPuestoCliente, PuestoCliente>();
builder.Services.AddScoped<IEmpleadoCliente, EmpleadoCliente>();
builder.Services.AddScoped<IEmpleadoConceptoNominaCliente, EmpleadoConceptoNominaCliente>();
builder.Services.AddScoped<ITipoPermisoCliente, TipoPermisoCliente>();
builder.Services.AddScoped<ITipoIncapacidadCliente, TipoIncapacidadCliente>();
builder.Services.AddScoped<ITipoHoraExtraCliente, TipoHoraExtraCliente>();
builder.Services.AddScoped<IAsistenciaCliente, AsistenciaCliente>();
builder.Services.AddScoped<IUsuarioCliente, UsuarioCliente>();
builder.Services.AddScoped<IPermisoCliente, PermisoCliente>();
builder.Services.AddScoped<ISolicitudVacacionesCliente, SolicitudVacacionesCliente>();
builder.Services.AddScoped<ISolicitudHorasExtraCliente, SolicitudHorasExtraCliente>();
builder.Services.AddScoped<IIncapacidadCliente, IncapacidadCliente>();
builder.Services.AddScoped<INotificacionCliente, NotificacionCliente>();
builder.Services.AddScoped<ISolicitudesCliente, SolicitudesCliente>();
builder.Services.AddScoped<IDepartamentoJefaturaCliente, DepartamentoJefaturaCliente>();
builder.Services.AddScoped<IEmpleadoJerarquiaCliente, EmpleadoJerarquiaCliente>();
builder.Services.AddScoped<IModoCalculoConceptoNominaCliente, ModoCalculoConceptoNominaCliente>();
builder.Services.AddScoped<ITipoConceptoNominaCliente, TipoConceptoNominaCliente>();
builder.Services.AddScoped<ITipoPlanillaCliente, TipoPlanillaCliente>();
builder.Services.AddScoped<ITipoPlanillaConceptoCliente, TipoPlanillaConceptoCliente>();
builder.Services.AddScoped<IFlujoEstadoCliente, FlujoEstadoCliente>();
builder.Services.AddScoped<IPlanillaEncabezadoCliente, PlanillaEncabezadoCliente>();
builder.Services.AddScoped<IMiPlanillaCliente, MiPlanillaCliente>();
builder.Services.AddScoped<ITramoRentaSalarioCliente, TramoRentaSalarioCliente>();
builder.Services.AddScoped<ApiErrorState>();


//Servicios Radzen
builder.Services.AddScoped<Radzen.DialogService>();
builder.Services.AddScoped<Radzen.NotificationService>();
builder.Services.AddScoped<Radzen.TooltipService>();
builder.Services.AddScoped<Radzen.ContextMenuService>();

builder.Services.AddAuthentication("Dummy")
    .AddScheme<AuthenticationSchemeOptions, DummyAuthHandler>(
        "Dummy", options => { });

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context => RolesSistema.EsAdministrador(context.User)));
});
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>(sp =>
    (CustomAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<ProtectedLocalStorage>();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7068/"); 
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("ApiClient");
});


/*builder.Services.AddAuthentication("ManualAuth") 
    .AddCookie("ManualAuth", options =>
    {
        options.LoginPath = "/login"; 
        options.LogoutPath = "/login";
    });*/


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
//app.UseAuthentication();
//app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();




app.Run();
