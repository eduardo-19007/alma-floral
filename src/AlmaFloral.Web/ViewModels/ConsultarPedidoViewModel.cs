using System.ComponentModel.DataAnnotations;
using AlmaFloral.Web.Models;
namespace AlmaFloral.Web.ViewModels;
public class ConsultarPedidoViewModel
{
    [Required(ErrorMessage = "Ingresa el código de tu pedido.")]
    [RegularExpression(@"^AF-[A-Fa-f0-9]{24}$", ErrorMessage = "El código debe comenzar con AF- seguido de 24 caracteres.")]
    [Display(Name = "Código del pedido")]
    private string _codigo = string.Empty;
    public string Codigo
    {
        get => _codigo;
        set => _codigo = (value ?? string.Empty).Trim().ToUpperInvariant();
    }
    [Required(ErrorMessage = "Ingresa el teléfono registrado.")]
    [RegularExpression(@"^\+?[0-9]{7,15}$", ErrorMessage = "Ingresa el teléfono sin espacios, entre 7 y 15 dígitos.")]
    [Display(Name = "Teléfono registrado")]
    public string Telefono { get; set; } = string.Empty;
    public List<CasoPostventa> Casos { get; set; } = new();
    public Pedido? Resultado { get; set; }
}