using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AlmaFloral.Web.Data;
using AlmaFloral.Web.Services;
using AlmaFloral.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AlmaFloral.Web.Controllers;
[Authorize(Roles="Administrador")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class AdministracionController(AlmaFloralDbContext db,GestionService servicio):Controller
{
 public async Task<IActionResult> Index(CancellationToken ct){ViewData["Productos"]=await db.Productos.CountAsync(ct);ViewData["Pedidos"]=await db.Pedidos.CountAsync(p=>p.Estado!="Entregado"&&p.Estado!="Cancelado",ct);ViewData["Casos"]=await db.CasosPostventa.CountAsync(c=>c.Estado!="Cerrado",ct);return View();}
 public async Task<IActionResult> Productos(CancellationToken ct)=>View(await db.Productos.AsNoTracking().OrderBy(p=>p.Id).ToListAsync(ct));
 [HttpGet] public async Task<IActionResult> Producto(int id,CancellationToken ct){if(id==0)return View(new ProductoEditarViewModel());var p=await db.Productos.AsNoTracking().SingleOrDefaultAsync(p=>p.Id==id,ct);return p is null?NotFound():View(new ProductoEditarViewModel{Id=p.Id,Nombre=p.Nombre,Descripcion=p.Descripcion,Precio=p.Precio,StockFisico=p.StockFisico,StockReservado=p.StockReservado,Activo=p.Activo});}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Producto([Bind("Id,Nombre,Descripcion,Precio,StockFisico,Activo")] ProductoEditarViewModel f,CancellationToken ct){if(ModelState.IsValid){try{await servicio.GuardarProductoAsync(f,ct);TempData["Mensaje"]="Producto guardado. Se conserva el historial de pedidos.";return RedirectToAction(nameof(Productos));}catch(ValidationException e){ModelState.AddModelError("",e.Message);}}f.StockReservado=await db.Productos.Where(p=>p.Id==f.Id).Select(p=>p.StockReservado).FirstOrDefaultAsync(ct);return View(f);}
 public async Task<IActionResult> Pedidos(CancellationToken ct)=>View(await db.Pedidos.AsNoTracking().OrderByDescending(p=>p.PedidoId).Take(200).ToListAsync(ct));
 public async Task<IActionResult> Pedido(int id,CancellationToken ct){var p=await db.Pedidos.AsNoTracking().Include(p=>p.Cliente).Include(p=>p.Detalles).ThenInclude(d=>d.Producto).SingleOrDefaultAsync(p=>p.PedidoId==id,ct);return p is null?NotFound():View(p);}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> EstadoPedido(int id,string estado,CancellationToken ct){try{await servicio.CambiarEstadoAsync(id,estado,ct);TempData["Mensaje"]="Estado actualizado y existencias verificadas.";}catch(ValidationException e){TempData["Error"]=e.Message;}return RedirectToAction(nameof(Pedido),new{id});}
 public async Task<IActionResult> Casos(CancellationToken ct)=>View(await db.CasosPostventa.AsNoTracking().Include(c=>c.Pedido).OrderByDescending(c=>c.CasoPostventaId).Take(200).ToListAsync(ct));
 [HttpGet] public async Task<IActionResult> Caso(int id,CancellationToken ct){var c=await db.CasosPostventa.AsNoTracking().Include(c=>c.Pedido).Include(c=>c.Usuario).SingleOrDefaultAsync(c=>c.CasoPostventaId==id,ct);if(c is null)return NotFound();ViewData["Caso"]=c;return View(new AtenderCasoViewModel{Id=id,Estado=c.Estado=="Abierto"?"En atención":c.Estado,Respuesta=c.Respuesta,Acuerdo=c.Acuerdo});}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Caso(AtenderCasoViewModel f,CancellationToken ct){if(ModelState.IsValid){try{await servicio.AtenderCasoAsync(f,int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),ct);TempData["Mensaje"]="Atención del caso actualizada.";return RedirectToAction(nameof(Caso),new{id=f.Id});}catch(ValidationException e){ModelState.AddModelError("",e.Message);}}var c=await db.CasosPostventa.AsNoTracking().Include(c=>c.Pedido).Include(c=>c.Usuario).SingleOrDefaultAsync(c=>c.CasoPostventaId==f.Id,ct);if(c is null)return NotFound();ViewData["Caso"]=c;return View(f);}
}
