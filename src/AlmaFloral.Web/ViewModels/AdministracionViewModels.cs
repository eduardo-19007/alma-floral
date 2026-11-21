using System.ComponentModel.DataAnnotations;
using AlmaFloral.Web.Models;
namespace AlmaFloral.Web.ViewModels;
public class IngresoViewModel
{
 [Required, EmailAddress, StringLength(254), Display(Name="Correo electrónico")] public string Correo { get; set; } = "";
 [Required, DataType(DataType.Password), StringLength(128), Display(Name="Contraseña")] public string Contrasena { get; set; } = "";
 public string? ReturnUrl { get; set; }
}
public class ProductoEditarViewModel
{
 public int Id { get; set; }
 [Required, StringLength(120)] public string Nombre { get; set; } = "";
 [Required, StringLength(500), Display(Name="Descripción")] public string Descripcion { get; set; } = "";
 [Range(typeof(decimal),"0.01","100000")] public decimal Precio { get; set; }
 [Range(0,int.MaxValue), Display(Name="Stock físico")] public int StockFisico { get; set; }
 public bool Activo { get; set; } = true;
 public int StockReservado { get; set; }
}
public class PostventaViewModel : ConsultarPedidoViewModel
{
 [Required, RegularExpression("^(Consulta|Reclamo|Solicitud)$")] public string Tipo { get; set; } = "Consulta";
 [Required(ErrorMessage="Describe tu solicitud."), StringLength(2000), Display(Name="Descripción")] public string Descripcion { get; set; } = "";
}
public class AtenderCasoViewModel
{
 public int Id { get; set; }
 [Required, RegularExpression("^(En atención|Cerrado)$")] public string Estado { get; set; } = "En atención";
 [StringLength(2000)] public string? Respuesta { get; set; }
 [StringLength(1000)] public string? Acuerdo { get; set; }
}
public class PedidoConsultaResultadoViewModel
{
 public Pedido Pedido { get; set; } = null!;
 public List<CasoPostventa> Casos { get; set; } = new();
}
