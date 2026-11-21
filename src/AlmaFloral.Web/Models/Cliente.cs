using System.ComponentModel.DataAnnotations;

namespace AlmaFloral.Web.Models;

public class Cliente
{
    public int ClienteId { get; set; }

    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(254)]
    public string? Correo { get; set; }
}