using System.Net;
using AlmaFloral.Web.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AlmaFloral.Web.Tests;

public class PaginaInicioTests :
    IClassFixture<WebApplicationFactory<HomeController>>,
    IDisposable
{
    private readonly HttpClient _cliente;

    public PaginaInicioTests(
        WebApplicationFactory<HomeController> fabrica)
    {
        _cliente = fabrica
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Home/Index")]
    public async Task Inicio_RespondeCorrectamente_YMuestraLaMarca(
        string ruta)
    {
        var respuesta = await _cliente.GetAsync(ruta);
        var contenido = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(
            "text/html",
            respuesta.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Alma Floral", contenido);
    }

    [Fact]
    public async Task RutaInexistente_Devuelve404()
    {
        var respuesta = await _cliente.GetAsync(
            "/ruta-que-no-existe-ap3");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    public void Dispose()
    {
        _cliente.Dispose();
    }
}
