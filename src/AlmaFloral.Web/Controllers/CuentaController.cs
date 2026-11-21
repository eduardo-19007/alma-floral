using System.Security.Claims;
using AlmaFloral.Web.Data;
using AlmaFloral.Web.Models;
using AlmaFloral.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
namespace AlmaFloral.Web.Controllers;
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class CuentaController(AlmaFloralDbContext db):Controller
{
 [HttpGet] public IActionResult Ingresar(string? returnUrl=null)=>View(new IngresoViewModel{ReturnUrl=returnUrl});
 [HttpPost,ValidateAntiForgeryToken,EnableRateLimiting("formularios")]
 public async Task<IActionResult> Ingresar(IngresoViewModel f,CancellationToken ct)
 {
  if(!ModelState.IsValid)return View(f);
  var correo=f.Correo.Trim().ToLowerInvariant();
  var usuario=await db.Usuarios.SingleOrDefaultAsync(u=>u.Correo==correo&&u.Activo,ct);
  var hasher=new PasswordHasher<Usuario>();
  if(usuario is null){hasher.HashPassword(new Usuario(),f.Contrasena);ModelState.AddModelError("","Correo o contraseña incorrectos.");return View(f);}
  var resultado=hasher.VerifyHashedPassword(usuario,usuario.HashContrasena,f.Contrasena);
  if(resultado==PasswordVerificationResult.Failed){ModelState.AddModelError("","Correo o contraseña incorrectos.");return View(f);}
  if(resultado==PasswordVerificationResult.SuccessRehashNeeded){usuario.HashContrasena=hasher.HashPassword(usuario,f.Contrasena);await db.SaveChangesAsync(ct);}
  var identidad=new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,usuario.UsuarioId.ToString()),new Claim(ClaimTypes.Name,usuario.Nombre),new Claim(ClaimTypes.Role,usuario.Rol)},CookieAuthenticationDefaults.AuthenticationScheme);
  await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(identidad));
  return Url.IsLocalUrl(f.ReturnUrl)?LocalRedirect(f.ReturnUrl!):RedirectToAction("Index","Administracion");
 }
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Salir(){await HttpContext.SignOutAsync();return RedirectToAction("Index","Home");}
 [HttpGet] public IActionResult Denegado(){Response.StatusCode=403;return View();}
}
