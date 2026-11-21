using System.ComponentModel.DataAnnotations;

namespace AlmaFloral.Web.ViewModels;

public class NuevoPedidoViewModel : IValidatableObject
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Selecciona un producto válido.")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "Ingresa tu nombre.")]
    [StringLength(150)]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu teléfono.")]
    [RegularExpression(@"^\+?[0-9]{7,15}$",
        ErrorMessage = "Ingresa entre 7 y 15 dígitos, sin espacios. Puedes incluir + al inicio.")]
    [StringLength(20)]
    [Display(Name = "Teléfono de contacto")]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [StringLength(254)]
    [Display(Name = "Correo electrónico opcional")]
    public string? Correo { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public int Cantidad { get; set; } = 1;

    [Required(ErrorMessage = "Selecciona la modalidad de entrega.")]
    [RegularExpression(@"^(Delivery|Recojo)$",
        ErrorMessage = "Selecciona Delivery o Recojo.")]
    [Display(Name = "Modalidad de entrega")]
    public string ModalidadEntrega { get; set; } = "Recojo";

    [StringLength(300)]
    [Display(Name = "Dirección de entrega")]
    public string? DireccionEntrega { get; set; }

    [Required(ErrorMessage = "Selecciona la fecha y hora de entrega.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Fecha y hora de entrega o recojo")]
    public DateTime? FechaEntregaProgramada { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (ModalidadEntrega == "Delivery"
            && string.IsNullOrWhiteSpace(DireccionEntrega))
        {
            yield return new ValidationResult(
                "La dirección es obligatoria para delivery.",
                new[] { nameof(DireccionEntrega) });
        }
    }
}