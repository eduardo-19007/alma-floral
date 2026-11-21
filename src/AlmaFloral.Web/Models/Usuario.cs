using System.ComponentModel.DataAnnotations;
namespace AlmaFloral.Web.Models;
public class Usuario
{
 public int UsuarioId { get; set; }
 [Required, StringLength(150)] public string Nombre { get; set; } = "";
 [Required, EmailAddress, StringLength(254)] public string Correo { get; set; } = "";
 [Required, StringLength(500)] public string HashContrasena { get; set; } = "";
 [Required, StringLength(30)] public string Rol { get; set; } = "Administrador";
 public bool Activo { get; set; } = true;
}
