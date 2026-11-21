using System.ComponentModel.DataAnnotations;
using AlmaFloral.Web.Services;
using AlmaFloral.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace AlmaFloral.Web.Controllers;
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class PostventaController(GestionService servicio):Controller
{
 [HttpGet] public IActionResult Nueva()=>View(new PostventaViewModel());
 [HttpPost,ValidateAntiForgeryToken,EnableRateLimiting("formularios")]
 public async Task<IActionResult> Nueva([Bind("Codigo,Telefono,Tipo,Descripcion")] PostventaViewModel f,CancellationToken ct)
 {
  if(ModelState.IsValid)try{var id=await servicio.AbrirCasoAsync(f,ct);TempData["CasoCreado"]=id;return RedirectToAction(nameof(Confirmacion));}catch(ValidationException e){ModelState.AddModelError("",e.Message);}
  return View(f);
 }
 public IActionResult Confirmacion(){if(TempData.Peek("CasoCreado") is not int id)return RedirectToAction(nameof(Nueva));ViewData["CasoId"]=id;return View();}
}
