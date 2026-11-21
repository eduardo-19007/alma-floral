using System.ComponentModel.DataAnnotations;

namespace AlmaFloral.Web.Models;

public class Pedido
{
    public int PedidoId { get; set; }

    public int ClienteId { get; set; }

    public Cliente Cliente { get; set; } = null!;

    [Required]
    [StringLength(30)]
    public string Codigo { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Pendiente";

    [Required]
    [StringLength(10)]
    public string ModalidadEntrega { get; set; } = string.Empty;

    [StringLength(300)]
    public string? DireccionEntrega { get; set; }

    public DateTime FechaEntregaProgramada { get; set; }

    public List<DetallePedido> Detalles { get; set; } = new();
}