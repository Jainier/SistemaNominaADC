using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Win32;
using SistemaNominaADC.Presentacion.Components;
using SistemaNominaADC.Presentacion.Security;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;

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


//Servicios Radzen
builder.Services.AddScoped<Radzen.DialogService>();
builder.Services.AddScoped<Radzen.NotificationService>();
builder.Services.AddScoped<Radzen.TooltipService>();
builder.Services.AddScoped<Radzen.ContextMenuService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Dummy";
    options.DefaultChallengeScheme = "Dummy";
})
.AddScheme<AuthenticationSchemeOptions, DummyAuthHandler>(
    "Dummy", options => { });

builder.Services.AddAuthorizationCore();
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
