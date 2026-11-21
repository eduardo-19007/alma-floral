using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;
using AlmaFloral.Web.Data;
using AlmaFloral.Web.Models;
using AlmaFloral.Web.Services;
using AlmaFloral.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace AlmaFloral.Web.Tests;
public class GestionCompletaTests
{
 private static NuevoPedidoViewModel Pedido()=>new(){ProductoId=1,Cantidad=2,Nombre="Cliente ficticio",Telefono="999123456",ModalidadEntrega="Recojo",FechaEntregaProgramada=DateTime.UtcNow.AddDays(2)};
 private static Usuario Admin(AlmaFloralDbContext db){var u=new Usuario{Nombre="Administrador prueba",Correo="admin@prueba.test"};u.HashContrasena=new PasswordHasher<Usuario>().HashPassword(u,"ClavePrueba-2026!");db.Usuarios.Add(u);db.SaveChanges();return u;}
 [Fact] public async Task Cancelar_LiberaReserva_SinReducirStockFisico()
 {
  await using var f=new AplicacionPruebas();using var scope=f.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
  await new PedidoService(db).RegistrarAsync(Pedido());var id=(await db.Pedidos.SingleAsync()).PedidoId;
  await new GestionService(db).CambiarEstadoAsync(id,"Cancelado");db.ChangeTracker.Clear();var p=await db.Productos.SingleAsync(p=>p.Id==1);
  Assert.Equal(10,p.StockFisico);Assert.Equal(0,p.StockReservado);Assert.Equal("Cancelado",(await db.Pedidos.SingleAsync()).Estado);
  await Assert.ThrowsAsync<ValidationException>(()=>new GestionService(db).CambiarEstadoAsync(id,"Cancelado"));
 }
 [Fact] public async Task Entregar_DescuentaStockUnaSolaVez()
 {
  await using var f=new AplicacionPruebas();using var scope=f.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
  await new PedidoService(db).RegistrarAsync(Pedido());var id=(await db.Pedidos.SingleAsync()).PedidoId;var s=new GestionService(db);
  foreach(var estado in new[]{"En preparación","Listo","Entregado"})await s.CambiarEstadoAsync(id,estado);
  db.ChangeTracker.Clear();var p=await db.Productos.SingleAsync(p=>p.Id==1);Assert.Equal(8,p.StockFisico);Assert.Equal(0,p.StockReservado);
  await Assert.ThrowsAsync<ValidationException>(()=>s.CambiarEstadoAsync(id,"Entregado"));Assert.Equal(8,(await db.Productos.AsNoTracking().SingleAsync(p=>p.Id==1)).StockFisico);
 }
 [Theory][InlineData("Entregado")][InlineData("Listo")][InlineData("Desconocido")]
 public async Task Estado_NoPermiteSaltarEtapas(string estado)
 {
  await using var f=new AplicacionPruebas();using var scope=f.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
  await new PedidoService(db).RegistrarAsync(Pedido());var id=(await db.Pedidos.SingleAsync()).PedidoId;
  await Assert.ThrowsAsync<ValidationException>(()=>new GestionService(db).CambiarEstadoAsync(id,estado));Assert.Equal("Pendiente",(await db.Pedidos.AsNoTracking().SingleAsync()).Estado);
 }
 [Fact] public async Task Cancelar_DespuesDePreparar_SeRechaza()
 {
  await using var f=new AplicacionPruebas();using var scope=f.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
  await new PedidoService(db).RegistrarAsync(Pedido());var id=(await db.Pedidos.SingleAsync()).PedidoId;var s=new GestionService(db);
  await s.CambiarEstadoAsync(id,"En preparación");await Assert.ThrowsAsync<ValidationException>(()=>s.CambiarEstadoAsync(id,"Cancelado"));
  Assert.Equal(2,(await db.Productos.AsNoTracking().SingleAsync(p=>p.Id==1)).StockReservado);
 }
 [Fact] public async Task FalloDeStock_RevierteCambioDeEstado()
 {
  await using var f=new AplicacionPruebas();using var scope=f.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
  await new PedidoService(db).RegistrarAsync(Pedido());var id=(await db.Pedidos.SingleAsync()).PedidoId;
  await db.Productos.Where(p=>p.Id==1).ExecuteUpdateAsync(s=>s.SetProperty(p=>p.StockReservado,0));
  await Assert.ThrowsAsync<ValidationException>(()=>new GestionService(db).CambiarEstadoAsync(id,"Cancelado"));
  Assert.Equal("Pendiente",(await db.Pedidos.AsNoTracking().SingleAsync()).Estado);
 }
 [Fact] public async Task Producto_NoPermiteReducirStockBajoReservas()
 {
  await using var f=new AplicacionPruebas();using var scope=f.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
  await new PedidoService(db).RegistrarAsync(Pedido());
  await Assert.ThrowsAsync<ValidationException>(()=>new GestionService(db).GuardarProductoAsync(new(){Id=1,Nombre="Ramo",Descripcion="Descripción",Precio=85,StockFisico=1}));
  Assert.Equal(10,(await db.Productos.AsNoTracking().SingleAsync(p=>p.Id==1)).StockFisico);
 }
 [Fact] public async Task Producto_PermiteCrearEditarYDesactivarSinBorrarHistorial()
 {
  await using var f=new AplicacionPruebas();using var scope=f.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();var s=new GestionService(db);
  await s.GuardarProductoAsync(new(){Nombre="Producto ficticio",Descripcion="Prueba",Precio=55,StockFisico=3});
  var nuevo=await db.Productos.AsNoTracking().SingleAsync(p=>p.Nombre=="Producto ficticio");Assert.True(nuevo.Activo);
  await new PedidoService(db).RegistrarAsync(Pedido());
  await s.GuardarProductoAsync(new(){Id=1,Nombre="Ramo actualizado",Descripcion="Prueba",Precio=90,StockFisico=10,Activo=false});
  Assert.False((await db.Productos.AsNoTracking().SingleAsync(p=>p.Id==1)).Activo);Assert.Equal(1,await db.Pedidos.CountAsync());
  Assert.Equal(85m,(await db.DetallesPedido.SingleAsync()).PrecioUnitario);
 }
 [Fact] public async Task Postventa_AbreAtiendeYCierraConResponsable()
 {
  await using var f=new AplicacionPruebas();using var scope=f.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();var admin=Admin(db);
  var codigo=await new PedidoService(db).RegistrarAsync(Pedido());var s=new GestionService(db);
  var id=await s.AbrirCasoAsync(new(){Codigo=codigo,Telefono="999123456",Descripcion="Consulta ficticia",Tipo="Consulta"});
  await Assert.ThrowsAsync<ValidationException>(()=>s.AtenderCasoAsync(new(){Id=id,Estado="Cerrado",Respuesta="Respuesta",Acuerdo="Acuerdo"},admin.UsuarioId));
  await s.AtenderCasoAsync(new(){Id=id,Estado="En atención"},admin.UsuarioId);
  await Assert.ThrowsAsync<ValidationException>(()=>s.AtenderCasoAsync(new(){Id=id,Estado="Cerrado"},admin.UsuarioId));
  await s.AtenderCasoAsync(new(){Id=id,Estado="Cerrado",Respuesta="Consulta atendida",Acuerdo="Cliente informado"},admin.UsuarioId);
  var caso=await db.CasosPostventa.AsNoTracking().SingleAsync();Assert.Equal("Cerrado",caso.Estado);Assert.NotNull(caso.FechaCierre);Assert.Equal(admin.UsuarioId,caso.UsuarioId);
  await Assert.ThrowsAsync<ValidationException>(()=>s.AtenderCasoAsync(new(){Id=id,Estado="En atención"},admin.UsuarioId));
 }
 [Fact] public async Task Postventa_TelefonoIncorrecto_NoCreaCaso()
 {
  await using var f=new AplicacionPruebas();using var scope=f.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
  var codigo=await new PedidoService(db).RegistrarAsync(Pedido());
  await Assert.ThrowsAsync<ValidationException>(()=>new GestionService(db).AbrirCasoAsync(new(){Codigo=codigo,Telefono="999000000",Descripcion="Prueba"}));Assert.Equal(0,await db.CasosPostventa.CountAsync());
 }
 [Theory][InlineData("/Administracion")][InlineData("/Administracion/Productos")][InlineData("/Administracion/Pedidos")][InlineData("/Administracion/Casos")]
 public async Task Administracion_SinSesion_ExigeIngreso(string ruta)
 {
  await using var f=new AplicacionPruebas();using var c=f.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
  var r=await c.GetAsync(ruta);Assert.Equal(HttpStatusCode.Redirect,r.StatusCode);Assert.Contains("/Cuenta/Ingresar",r.Headers.Location!.ToString());
 }
 [Fact] public async Task Ingreso_ValidaCredencialesYCierreDeSesion()
 {
  await using var f=new AplicacionPruebas();using(var scope=f.Services.CreateScope()){Admin(scope.ServiceProvider.GetRequiredService<AlmaFloralDbContext>());}
  using var c=f.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
  var html=await c.GetStringAsync("/Cuenta/Ingresar");var token=Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
  var campos=new Dictionary<string,string>{{"__RequestVerificationToken",WebUtility.HtmlDecode(token)},{"Correo","admin@prueba.test"},{"Contrasena","Incorrecta"}};
  Assert.Equal(HttpStatusCode.OK,(await c.PostAsync("/Cuenta/Ingresar",new FormUrlEncodedContent(campos))).StatusCode);
  campos["Contrasena"]="ClavePrueba-2026!";campos["ReturnUrl"]="https://ejemplo.test/externo";
  var r=await c.PostAsync("/Cuenta/Ingresar",new FormUrlEncodedContent(campos));Assert.Equal("/Administracion",r.Headers.Location!.ToString());
  html=await c.GetStringAsync("/Administracion");Assert.Contains("Productos registrados",html);
  token=Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
  await c.PostAsync("/Cuenta/Salir",new FormUrlEncodedContent(new Dictionary<string,string>{{"__RequestVerificationToken",WebUtility.HtmlDecode(token)}}));
  Assert.Equal(HttpStatusCode.Redirect,(await c.GetAsync("/Administracion")).StatusCode);
 }
}
