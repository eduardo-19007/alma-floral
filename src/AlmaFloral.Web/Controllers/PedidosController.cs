using System.ComponentModel.DataAnnotations;
using AlmaFloral.Web.Data;
using AlmaFloral.Web.Models;
using AlmaFloral.Web.Services;
using AlmaFloral.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmaFloral.Web.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PedidosController : Controller
{
    private readonly AlmaFloralDbContext _contexto;
    private readonly PedidoService _servicio;
    private readonly ILogger<PedidosController> _logger;

    public PedidosController(
        AlmaFloralDbContext contexto,
        PedidoService servicio,
        ILogger<PedidosController> logger)
    {
        _contexto = contexto;
        _servicio = servicio;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Nuevo(
        int productoId,
        CancellationToken cancellationToken)
    {
        var producto = await BuscarProductoAsync(
            productoId, cancellationToken);

        if (producto is null)
        {
            return NotFound();
        }

        ViewData["Producto"] = producto;

        return View(new NuevoPedidoViewModel
        {
            ProductoId = producto.Id
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nuevo(
        NuevoPedidoViewModel formulario,
        CancellationToken cancellationToken)
    {
        var producto = await BuscarProductoAsync(
            formulario.ProductoId, cancellationToken);

        if (producto is null)
        {
            return NotFound();
        }

        ViewData["Producto"] = producto;

        if (!ModelState.IsValid)
        {
            return View(formulario);
        }

        try
        {
            var codigo = await _servicio.RegistrarAsync(
                formulario, cancellationToken);

            TempData["PedidoCreado"] = codigo;

            return RedirectToAction(nameof(Confirmacion));
        }
        catch (ValidationException excepcion)
        {
            ModelState.AddModelError(
                string.Empty, excepcion.Message);

            // Volver a consultar la disponibilidad actual.
            var actualizado = await BuscarProductoAsync(
                formulario.ProductoId, cancellationToken);

            if (actualizado is null)
            {
                return NotFound();
            }

            ViewData["Producto"] = actualizado;
        }
        catch (DbUpdateException excepcion)
        {
            _logger.LogError(
                excepcion, "No se pudo guardar el pedido.");

            ModelState.AddModelError(
                string.Empty,
                "No se pudo guardar el pedido. Inténtalo nuevamente.");
        }

        return View(formulario);
    }

    [HttpGet]
    public async Task<IActionResult> Confirmacion(
        CancellationToken cancellationToken)
    {
        if (TempData.Peek("PedidoCreado") is not string codigo)
        {
            return RedirectToAction("Index", "Catalogo");
        }

        var pedido = await _contexto.Pedidos
            .AsNoTracking()
            .Include(p => p.Detalles)
            .ThenInclude(detalle => detalle.Producto)
            .SingleOrDefaultAsync(
                p => p.Codigo == codigo,
                cancellationToken);

        if (pedido is null)
        {
            return NotFound();
        }

        return View(pedido);
    }

    [HttpGet]
    public IActionResult Consultar() => View(new ConsultarPedidoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Consultar(
        [Bind("Codigo,Telefono")] ConsultarPedidoViewModel formulario,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(formulario);
        var codigo = formulario.Codigo.ToUpperInvariant();
        var telefono = formulario.Telefono.TrimStart('+');
        formulario.Resultado = await _contexto.Pedidos.AsNoTracking()
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .SingleOrDefaultAsync(p => p.Codigo == codigo && p.Cliente.Telefono == telefono,
                cancellationToken);
        if (formulario.Resultado is not null)
            formulario.Casos = await _contexto.CasosPostventa.AsNoTracking().Where(c => c.PedidoId == formulario.Resultado.PedidoId).OrderByDescending(c => c.CasoPostventaId).ToListAsync(cancellationToken);
        if (formulario.Resultado is null)
            ModelState.AddModelError(string.Empty, "No se encontró un pedido con esos datos. Verifica el código y el teléfono.");
        return View(formulario);
    }

    private Task<Producto?> BuscarProductoAsync(
        int productoId,
        CancellationToken cancellationToken)
    {
        return _contexto.Productos
            .AsNoTracking()
            .SingleOrDefaultAsync(
                producto => producto.Id == productoId
                    && producto.Activo,
                cancellationToken);
    }
}