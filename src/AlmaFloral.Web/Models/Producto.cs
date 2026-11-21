using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaFloral.Web.Models;

public class Producto
{
    [Column("ProductoId")]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    [Range(typeof(decimal), "0.01", "100000",
        ErrorMessage = "El precio debe estar entre S/ 0.01 y S/ 100,000.")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "Las existencias no pueden ser negativas.")]
    public int StockFisico { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "Las reservas no pueden ser negativas.")]
    public int StockReservado { get; set; }

    [NotMapped]
    public int Stock => StockFisico - StockReservado;

    public bool Activo { get; set; } = true;
}