using System.ComponentModel.DataAnnotations;
namespace AlmaFloral.Web.Models;
public class CasoPostventa
{
 public int CasoPostventaId { get; set; }
 public int PedidoId { get; set; }
 public Pedido Pedido { get; set; } = null!;
 public int? UsuarioId { get; set; }
 public Usuario? Usuario { get; set; }
 [Required, StringLength(30)] public string Tipo { get; set; } = "Consulta";
 [Required, StringLength(2000)] public string Descripcion { get; set; } = "";
 [Required, StringLength(20)] public string Estado { get; set; } = "Abierto";
 public DateTime FechaApertura { get; set; } = DateTime.UtcNow;
 public DateTime? FechaCierre { get; set; }
 [StringLength(2000)] public string? Respuesta { get; set; }
 [StringLength(1000)] public string? Acuerdo { get; set; }
}
