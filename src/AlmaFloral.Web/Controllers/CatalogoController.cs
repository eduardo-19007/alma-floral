using AlmaFloral.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlmaFloral.Web.Controllers;

public class CatalogoController : Controller
{
    public IActionResult Index()
    {
        var productos = new List<Producto>
        {
            new Producto
            {
                Id = 1,
                Nombre = "Ramo de rosas",
                Descripcion = "Arreglo de rosas para ocasiones especiales.",
                Precio = 85.00m,
                Stock = 10,
                Activo = true
            },
            new Producto
            {
                Id = 2,
                Nombre = "Orquídea en maceta",
                Descripcion = "Planta decorativa presentada en maceta.",
                Precio = 120.00m,
                Stock = 5,
                Activo = true
            },
            new Producto
            {
                Id = 3,
                Nombre = "Arreglo floral mixto",
                Descripcion = "Composición de flores de distintos colores.",
                Precio = 95.00m,
                Stock = 0,
                Activo = true
            }
        };

        var productosActivos = productos
            .Where(producto => producto.Activo)
            .ToList();

        return View(productosActivos);
    }
}