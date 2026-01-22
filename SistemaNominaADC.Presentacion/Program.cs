using Microsoft.AspNetCore.Components.Authorization;
using SistemaNominaADC.Presentacion.Components;
using SistemaNominaADC.Presentacion.Security;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


//Servicios API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7068/")
});

builder.Services.AddScoped<IRolCliente, RolCliente>();
builder.Services.AddScoped<IObjetoSistemaCliente, ObjetoSistemaCliente>();
builder.Services.AddScoped<IGrupoEstadoCliente, GrupoEstadoCliente>();
builder.Services.AddScoped<IEstadoCliente, EstadoCliente>();
builder.Services.AddScoped<IDepartamentoCliente, DepartamentoCliente>();
builder.Services.AddScoped<SessionService>();


//Servicios Radzen
builder.Services.AddScoped<Radzen.DialogService>();
builder.Services.AddScoped<Radzen.NotificationService>();
builder.Services.AddScoped<Radzen.TooltipService>();
builder.Services.AddScoped<Radzen.ContextMenuService>();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();



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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

/*
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Definir los roles que tu sistema de nómina necesita
        string[] roleNames = { "Administrador", "Recursos Humanos", "Empleado" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Crea los roles en la tabla AspNetRoles
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Crear un usuario Administrador por defecto (opcional pero recomendado)
        var adminEmail = "admin@admin.com";
        var user = await userManager.FindByEmailAsync(adminEmail);

        if (user == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createPowerUser = await userManager.CreateAsync(adminUser, "Admin123*");
            if (createPowerUser.Succeeded)
            {
                // Asignar el rol de Administrador al usuario creado
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
            else
            {
                // ESTO TE DIRÁ POR QUÉ FALLÓ:
                foreach (var error in createPowerUser.Errors)
                {
                    Console.WriteLine($"Error creando usuario: {error.Description}");
                    // O usa un Debug.WriteLine o un breakpoint aquí
                }
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al sembrar la base de datos.");
    }
}
*/
app.Run();
