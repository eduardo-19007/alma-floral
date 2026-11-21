using AlmaFloral.Web.Controllers;
using AlmaFloral.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AlmaFloral.Web.Tests;

public class AplicacionPruebas : WebApplicationFactory<HomeController>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AlmaFloralDbContext>();
            services.RemoveAll<DbContextOptions<AlmaFloralDbContext>>();
            services.RemoveAll<
                IDbContextOptionsConfiguration<AlmaFloralDbContext>>();

            services.AddSingleton<SqliteConnection>(_ =>
            {
                var conexion = new SqliteConnection("Data Source=:memory:");
                conexion.Open();

                // Adaptar la función LEN usada por una restricción.
                conexion.CreateFunction<string?, int?>(
                    "LEN",
                    texto => texto?.TrimEnd(' ').Length,
                    isDeterministic: true);

                return conexion;
            });

            services.AddDbContext<AlmaFloralDbContext>((proveedor, opciones) =>
                opciones.UseSqlite(
                    proveedor.GetRequiredService<SqliteConnection>()));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var alcance = host.Services.CreateScope();
        var contexto = alcance.ServiceProvider
            .GetRequiredService<AlmaFloralDbContext>();

        contexto.Database.EnsureCreated();

        return host;
    }
}