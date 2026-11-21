using System.ComponentModel.DataAnnotations;
using AlmaFloral.Web.Data;
using AlmaFloral.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlmaFloral.Web.Services;

public static class AdministradorInicial
{
    public static async Task CrearAsync(
        IServiceProvider servicios, IConfiguration config, bool requerido = false)
    {
        var correo = config["Administracion:Correo"]?.Trim().ToLowerInvariant();
        var clave = config["Administracion:ContrasenaInicial"];
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
        {
            if (requerido)
                throw new InvalidOperationException("Configure el correo y la contraseña inicial mediante configuración privada.");
            return;
        }
        if (correo.Length > 254 || !new EmailAddressAttribute().IsValid(correo))
            throw new InvalidOperationException("El correo del administrador no es válido.");

        using var alcance = servicios.CreateScope();
        var db = alcance.ServiceProvider.GetRequiredService<AlmaFloralDbContext>();
        if (await db.Usuarios.AnyAsync(u => u.Correo == correo)) return;
        if (clave.Length < 12)
            throw new InvalidOperationException("La contraseña inicial debe tener al menos 12 caracteres.");

        var usuario = new Usuario { Nombre = "Administración Alma Floral", Correo = correo };
        usuario.HashContrasena = new PasswordHasher<Usuario>().HashPassword(usuario, clave);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
    }
}
