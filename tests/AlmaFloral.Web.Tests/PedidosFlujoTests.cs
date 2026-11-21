using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using AlmaFloral.Web.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AlmaFloral.Web.Tests;

public class PedidosFlujoTests
{
    private static HttpClient Cliente(AplicacionPruebas fabrica) => fabrica.CreateClient(
        new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

    private static async Task<Dictionary<string, string>> Campos(HttpClient cliente, string ruta)
    {
        var html = await cliente.GetStringAsync(ruta);
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(token.Success, "Debe generarse el token antifalsificación.");
        return new Dictionary<string, string> { ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token.Groups[1].Value) };
    }

    [Theory]
    [InlineData("/Pedidos/Nuevo?productoId=1")]
    [InlineData("/Pedidos/Consultar")]
    [InlineData("/Home/Privacy")]
    public async Task Pantallas_RespondenCorrectamente(string ruta)
    {
        await using var fabrica = new AplicacionPruebas();
        using var cliente = Cliente(fabrica);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync(ruta)).StatusCode);
    }

    [Fact]
    public async Task Registro_ConfirmaYConsultaSinPublicarDatosPersonales()
    {
        await using var fabrica = new AplicacionPruebas();
        using var cliente = Cliente(fabrica);
        var campos = await Campos(cliente, "/Pedidos/Nuevo?productoId=1");
        campos["ProductoId"] = "1";
        campos["Nombre"] = "Persona Ficticia";
        campos["Telefono"] = "+999123456";
        campos["Correo"] = "";
        campos["Cantidad"] = "2";
        campos["ModalidadEntrega"] = "Delivery";
        campos["DireccionEntrega"] = "Direccion ficticia de prueba 123";
        campos["FechaEntregaProgramada"] = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
        var respuesta = await cliente.PostAsync("/Pedidos/Nuevo", new FormUrlEncodedContent(campos));
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/Pedidos/Confirmacion", respuesta.Headers.Location?.OriginalString);
        var confirmacion = await cliente.GetStringAsync("/Pedidos/Confirmacion");
        Assert.Contains("Tu pedido ya", confirmacion);
        Assert.Contains("170.00", confirmacion);
        Assert.DoesNotContain("Persona Ficticia", confirmacion);
        Assert.DoesNotContain("Direccion ficticia", confirmacion);
        using var alcance = fabrica.Services.CreateScope();
        var db = alcance.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
        var pedido = await db.Pedidos.SingleAsync();
        Assert.Equal(2, (await db.Productos.SingleAsync(p => p.Id == 1)).StockReservado);

        campos = await Campos(cliente, "/Pedidos/Consultar");
        campos["Codigo"] = pedido.Codigo.ToLowerInvariant();
        campos["Telefono"] = "999123456";
        var consulta = await cliente.PostAsync("/Pedidos/Consultar", new FormUrlEncodedContent(campos));
        var html = await consulta.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        Assert.Contains("Resumen del pedido", html);
        Assert.Contains("Pendiente", html);
        Assert.DoesNotContain("Persona Ficticia", html);
        campos["Telefono"] = "999000000";
        html = await (await cliente.PostAsync("/Pedidos/Consultar", new FormUrlEncodedContent(campos))).Content.ReadAsStringAsync();
        Assert.DoesNotContain("Resumen del pedido", html);
        Assert.Contains("Verifica el", html);
    }

    [Fact]
    public async Task Post_SinToken_NoRegistraPedido()
    {
        await using var fabrica = new AplicacionPruebas();
        using var cliente = Cliente(fabrica);
        var respuesta = await cliente.PostAsync("/Pedidos/Nuevo", new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        using var alcance = fabrica.Services.CreateScope();
        Assert.Equal(0, await alcance.ServiceProvider.GetRequiredService<AlmaFloralDbContext>().Pedidos.CountAsync());
    }

    [Fact]
    public async Task Confirmacion_SinPedidoEnSesion_RedirigeAlCatalogo()
    {
        await using var fabrica = new AplicacionPruebas();
        using var cliente = Cliente(fabrica);
        var respuesta = await cliente.GetAsync("/Pedidos/Confirmacion");
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/Catalogo", respuesta.Headers.Location?.OriginalString);
    }
}
