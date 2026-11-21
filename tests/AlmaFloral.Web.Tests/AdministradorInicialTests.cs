using AlmaFloral.Web.Data;
using AlmaFloral.Web.Models;
using AlmaFloral.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AlmaFloral.Web.Tests;

public class AdministradorInicialTests
{
    [Fact]
    public async Task CreacionExplicita_SinSecretos_SeRechaza()
    {
        using var servicios = new ServiceCollection().BuildServiceProvider();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AdministradorInicial.CrearAsync(servicios, new ConfigurationBuilder().Build(), requerido: true));
    }

    [Fact]
    public async Task CreacionExplicita_GuardaHash_YNoReemplazaCredenciales()
    {
        using var fabrica = new AplicacionPruebas();
        _ = fabrica.Server;
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["Administracion:Correo"] = " ADMIN@EXAMPLE.TEST ",
            ["Administracion:ContrasenaInicial"] = "Prueba-privada-2026!"
        }).Build();
        await AdministradorInicial.CrearAsync(fabrica.Services, config, requerido: true);
        using var scope = fabrica.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
        var usuario = Assert.Single(db.Usuarios);
        var hash = usuario.HashContrasena;
        Assert.Equal("admin@example.test", usuario.Correo);
        Assert.Equal("Administrador", usuario.Rol);
        Assert.Equal(PasswordVerificationResult.Success,
            new PasswordHasher<Usuario>().VerifyHashedPassword(usuario, hash, "Prueba-privada-2026!"));
        config["Administracion:ContrasenaInicial"] = "Otra-clave-distinta-2026!";
        await AdministradorInicial.CrearAsync(fabrica.Services, config, requerido: true);
        Assert.Equal(hash, Assert.Single(db.Usuarios).HashContrasena);
    }

    [Fact]
    public async Task CreacionExplicita_ClaveCorta_NoCreaUsuario()
    {
        using var fabrica = new AplicacionPruebas();
        _ = fabrica.Server;
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["Administracion:Correo"] = "admin@example.test",
            ["Administracion:ContrasenaInicial"] = "corta"
        }).Build();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AdministradorInicial.CrearAsync(fabrica.Services, config, requerido: true));
        using var scope = fabrica.Services.CreateScope();
        Assert.Empty(scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>().Usuarios);
    }
}
