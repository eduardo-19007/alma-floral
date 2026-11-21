using System.ComponentModel.DataAnnotations;
using AlmaFloral.Web.Data;
using AlmaFloral.Web.Services;
using AlmaFloral.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AlmaFloral.Web.Tests;

public class PedidoServiceTests
{
    private static NuevoPedidoViewModel Formulario(int productoId=1,int cantidad=2)
        => new()
        {
            ProductoId=productoId,Cantidad=cantidad,Nombre="Cliente de prueba",
            Telefono="999123456",ModalidadEntrega="Recojo",
            FechaEntregaProgramada=DateTime.UtcNow.AddHours(-5).AddDays(1)
        };

    [Fact]
    public async Task Registrar_GuardaPedidoYReservaStock()
    {
        await using var fabrica=new AplicacionPruebas();
        using var alcance=fabrica.Services.CreateScope();
        var contexto=alcance.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
        var codigo=await new PedidoService(contexto).RegistrarAsync(Formulario());
        contexto.ChangeTracker.Clear();
        var pedido=await contexto.Pedidos.Include(p=>p.Detalles).SingleAsync();
        var producto=await contexto.Productos.SingleAsync(p=>p.Id==1);
        Assert.StartsWith("AF-",codigo);
        Assert.Equal(27,codigo.Length);
        Assert.Equal(codigo,pedido.Codigo);
        Assert.Equal(85m,Assert.Single(pedido.Detalles).PrecioUnitario);
        Assert.Equal(2,producto.StockReservado);
        Assert.Equal(8,producto.Stock);
        Assert.Equal(1,await contexto.Clientes.CountAsync());
    }

    [Theory]
    [InlineData(1,11)]
    [InlineData(3,1)]
    public async Task Registrar_SinExistencias_NoCreaRegistros(int productoId,int cantidad)
    {
        await using var fabrica=new AplicacionPruebas();
        using var alcance=fabrica.Services.CreateScope();
        var contexto=alcance.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
        await Assert.ThrowsAsync<ValidationException>(()=>
            new PedidoService(contexto).RegistrarAsync(Formulario(productoId,cantidad)));
        Assert.Equal(0,await contexto.Pedidos.CountAsync());
        Assert.Equal(0,await contexto.Clientes.CountAsync());
        Assert.All(await contexto.Productos.ToListAsync(),p=>Assert.Equal(0,p.StockReservado));
    }

    [Fact]
    public async Task Registrar_FechaPasada_NoReservaStock()
    {
        await using var fabrica=new AplicacionPruebas();
        using var alcance=fabrica.Services.CreateScope();
        var contexto=alcance.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
        var form=Formulario();form.FechaEntregaProgramada=DateTime.UtcNow.AddDays(-1);
        await Assert.ThrowsAsync<ValidationException>(()=>new PedidoService(contexto).RegistrarAsync(form));
        Assert.Equal(0,await contexto.Pedidos.CountAsync());
        Assert.All(await contexto.Productos.ToListAsync(),p=>Assert.Equal(0,p.StockReservado));
    }
    [Fact]
    public async Task Registrar_DeliverySinDireccion_NoReservaStock()
    {
        await using var fabrica = new AplicacionPruebas();
        using var alcance = fabrica.Services.CreateScope();
        var db = alcance.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
        var formulario = Formulario();
        formulario.ModalidadEntrega = "Delivery";
        formulario.DireccionEntrega = "   ";
        await Assert.ThrowsAsync<ValidationException>(() => new PedidoService(db).RegistrarAsync(formulario));
        Assert.Equal(0, await db.Pedidos.CountAsync());
        Assert.Equal(0, (await db.Productos.SingleAsync(p => p.Id == 1)).StockReservado);
    }

    [Fact]
    public async Task Registrar_ConservaPrecioHistoricoAunqueCambieElCatalogo()
    {
        await using var fabrica = new AplicacionPruebas();
        using var alcance = fabrica.Services.CreateScope();
        var db = alcance.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
        await new PedidoService(db).RegistrarAsync(Formulario());
        await db.Productos.Where(p => p.Id == 1).ExecuteUpdateAsync(c => c.SetProperty(p => p.Precio, 90m));
        db.ChangeTracker.Clear();
        Assert.Equal(85m, (await db.DetallesPedido.SingleAsync()).PrecioUnitario);
    }

    [Fact]
    public async Task Registrar_SegundaSolicitudSinStock_NoAgregaReservas()
    {
        await using var fabrica = new AplicacionPruebas();
        using var alcance = fabrica.Services.CreateScope();
        var db = alcance.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
        var servicio = new PedidoService(db);
        await servicio.RegistrarAsync(Formulario(1, 10));
        await Assert.ThrowsAsync<ValidationException>(() => servicio.RegistrarAsync(Formulario(1, 1)));
        db.ChangeTracker.Clear();
        Assert.Equal(1, await db.Pedidos.CountAsync());
        Assert.Equal(1, await db.Clientes.CountAsync());
        Assert.Equal(10, (await db.Productos.SingleAsync(p => p.Id == 1)).StockReservado);
    }
}
