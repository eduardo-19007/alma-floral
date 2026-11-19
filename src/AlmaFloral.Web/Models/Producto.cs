using System.ComponentModel.DataAnnotations;

namespace AlmaFloral.Web.Models;

public class Producto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, 100000,
        ErrorMessage = "El precio debe estar entre S/ 0.01 y S/ 100,000.")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "Las existencias no pueden ser negativas.")]
    public int Stock { get; set; }

    public bool Activo { get; set; } = true;
}