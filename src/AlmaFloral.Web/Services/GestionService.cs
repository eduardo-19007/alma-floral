using System.ComponentModel.DataAnnotations;
using AlmaFloral.Web.Data;
using AlmaFloral.Web.Models;
using AlmaFloral.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace AlmaFloral.Web.Services;
public class GestionService(AlmaFloralDbContext db)
{
 public async Task CambiarEstadoAsync(int id, string destino, CancellationToken ct=default)
 {
  await using var tx=await db.Database.BeginTransactionAsync(ct);
  var pedido=await db.Pedidos.AsNoTracking().Include(p=>p.Detalles).SingleOrDefaultAsync(p=>p.PedidoId==id,ct)
   ?? throw new ValidationException("No se encontró el pedido.");
  var permitido=(pedido.Estado,destino) switch {
   ("Pendiente","En preparación") or ("Pendiente","Cancelado") or
   ("En preparación","Listo") or ("Listo","Entregado") => true, _ => false };
  if(!permitido) throw new ValidationException("La transición no está permitida. Solo se cancela antes de iniciar la preparación.");
  var cambio=await db.Pedidos.Where(p=>p.PedidoId==id && p.Estado==pedido.Estado)
   .ExecuteUpdateAsync(s=>s.SetProperty(p=>p.Estado,destino),ct);
  if(cambio!=1) throw new ValidationException("El pedido cambió mientras lo revisabas. Actualiza la página.");
  if(destino is "Cancelado" or "Entregado") foreach(var d in pedido.Detalles.OrderBy(d=>d.ProductoId)) {
   var q=db.Productos.Where(p=>p.Id==d.ProductoId && p.StockReservado>=d.Cantidad && p.StockFisico>=d.Cantidad);
   int actualizados;
   if(destino=="Entregado") actualizados=await q.ExecuteUpdateAsync(s=>s
    .SetProperty(p=>p.StockReservado,p=>p.StockReservado-d.Cantidad)
    .SetProperty(p=>p.StockFisico,p=>p.StockFisico-d.Cantidad),ct);
   else actualizados=await q.ExecuteUpdateAsync(s=>s.SetProperty(p=>p.StockReservado,p=>p.StockReservado-d.Cantidad),ct);
   if(actualizados!=1) throw new ValidationException("No se pudo actualizar la reserva. No se guardó el cambio de estado.");
  }
  await tx.CommitAsync(ct);
 }
 public async Task GuardarProductoAsync(ProductoEditarViewModel f, CancellationToken ct=default)
 {
  Validator.ValidateObject(f,new ValidationContext(f),true);
  f.Nombre=f.Nombre.Trim();f.Descripcion=f.Descripcion.Trim();
  if(f.Id==0){db.Productos.Add(new Producto{Nombre=f.Nombre,Descripcion=f.Descripcion,Precio=f.Precio,StockFisico=f.StockFisico,Activo=f.Activo});await db.SaveChangesAsync(ct);return;}
  // One conditional update prevents an edit from invalidating a concurrent reservation.
  var n=await db.Productos.Where(p=>p.Id==f.Id && p.StockReservado<=f.StockFisico).ExecuteUpdateAsync(s=>s
   .SetProperty(p=>p.Nombre,f.Nombre).SetProperty(p=>p.Descripcion,f.Descripcion)
   .SetProperty(p=>p.Precio,f.Precio).SetProperty(p=>p.StockFisico,f.StockFisico).SetProperty(p=>p.Activo,f.Activo),ct);
  if(n!=1)throw new ValidationException("El stock físico no puede ser menor que las unidades reservadas. Actualiza los datos del producto.");
 }
 public async Task<int> AbrirCasoAsync(PostventaViewModel f,CancellationToken ct=default)
 {
  Validator.ValidateObject(f,new ValidationContext(f),true);
  var telefono=f.Telefono.TrimStart('+');
  var pedido=await db.Pedidos.AsNoTracking().SingleOrDefaultAsync(p=>p.Codigo==f.Codigo && p.Cliente.Telefono==telefono,ct)
   ?? throw new ValidationException("No se encontró un pedido con ese código y teléfono.");
  var caso=new CasoPostventa{PedidoId=pedido.PedidoId,Tipo=f.Tipo,Descripcion=f.Descripcion.Trim()};
  db.CasosPostventa.Add(caso);await db.SaveChangesAsync(ct);return caso.CasoPostventaId;
 }
 public async Task AtenderCasoAsync(AtenderCasoViewModel f,int usuarioId,CancellationToken ct=default)
 {
  Validator.ValidateObject(f,new ValidationContext(f),true);
  if(f.Estado=="Cerrado"&&(string.IsNullOrWhiteSpace(f.Respuesta)||string.IsNullOrWhiteSpace(f.Acuerdo)))
   throw new ValidationException("Para cerrar el caso registra la respuesta y el acuerdo o resolución.");
  var activo=await db.Usuarios.AnyAsync(u=>u.UsuarioId==usuarioId && u.Activo && u.Rol=="Administrador",ct);
  if(!activo)throw new ValidationException("El responsable no tiene acceso administrativo activo.");
  var caso=await db.CasosPostventa.AsNoTracking().SingleOrDefaultAsync(c=>c.CasoPostventaId==f.Id,ct)
   ?? throw new ValidationException("No se encontró el caso.");
  if(caso.Estado=="Cerrado" || (caso.Estado=="Abierto" && f.Estado!="En atención"))
   throw new ValidationException("Primero inicia la atención del caso. Los casos cerrados conservan su resolución.");
  var n=await db.CasosPostventa.Where(c=>c.CasoPostventaId==f.Id&&c.Estado==caso.Estado).ExecuteUpdateAsync(s=>s
   .SetProperty(c=>c.Estado,f.Estado).SetProperty(c=>c.UsuarioId,(int?)usuarioId)
   .SetProperty(c=>c.Respuesta,f.Respuesta).SetProperty(c=>c.Acuerdo,f.Acuerdo)
   .SetProperty(c=>c.FechaCierre,f.Estado=="Cerrado"?(DateTime?)DateTime.UtcNow:null),ct);
  if(n!=1)throw new ValidationException("El caso cambió mientras lo revisabas. Actualiza la página.");
 }
}
