using AlmaFloral.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmaFloral.Web.Controllers;

public class CatalogoController : Controller
{
    private readonly AlmaFloralDbContext _contexto;

    public CatalogoController(AlmaFloralDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var productos = await _contexto.Productos
            .AsNoTracking()
            .Where(producto => producto.Activo)
            .OrderBy(producto => producto.Id)
            .ToListAsync(cancellationToken);

        return View(productos);
    }
}