using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SistemaNominaADC.Datos;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = ResolverRutaBase();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("No se encontro la cadena de conexion 'DefaultConnection' para crear ApplicationDbContext en tiempo de diseno.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string ResolverRutaBase()
    {
        var actual = Directory.GetCurrentDirectory();
        for (var i = 0; i < 8; i++)
        {
            var apiPath = Path.Combine(actual, "SistemaNominaADC.Api");
            if (Directory.Exists(apiPath))
                return apiPath;

            var parent = Directory.GetParent(actual);
            if (parent is null)
                break;
            actual = parent.FullName;
        }

        throw new DirectoryNotFoundException("No se pudo ubicar la carpeta 'SistemaNominaADC.Api' para leer appsettings.");
    }
}
