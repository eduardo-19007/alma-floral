using AlmaFloral.Web.Data;
using AlmaFloral.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace AlmaFloral.Web.Services;
public static class AdministradorInicial
{
 public static async Task CrearAsync(IServiceProvider servicios,IConfiguration config)
 {
  var correo=config["Administracion:Correo"]?.Trim().ToLowerInvariant();
  var clave=config["Administracion:ContrasenaInicial"];
  if(string.IsNullOrWhiteSpace(correo)||string.IsNullOrWhiteSpace(clave))return;
  using var alcance=servicios.CreateScope();var db=alcance.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
  if(await db.Usuarios.AnyAsync(u=>u.Correo==correo))return;
  if(clave.Length<12)throw new InvalidOperationException("La contraseña inicial debe tener al menos 12 caracteres.");
  var u=new Usuario{Nombre="Administración Alma Floral",Correo=correo};
  u.HashContrasena=new PasswordHasher<Usuario>().HashPassword(u,clave);db.Usuarios.Add(u);await db.SaveChangesAsync();
 }
}
