using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using AlmaFloral.Web.Data;
using AlmaFloral.Web.Models;
using AlmaFloral.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AlmaFloral.Web.Services;

public class PedidoService
{
    private readonly AlmaFloralDbContext _contexto;

    public PedidoService(AlmaFloralDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<string> RegistrarAsync(
        NuevoPedidoViewModel formulario,
        CancellationToken cancellationToken = default)
    {
        Validator.ValidateObject(
            formulario,
            new ValidationContext(formulario),
            validateAllProperties: true);

        var zonaLima = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows()
                ? "SA Pacific Standard Time"
                : "America/Lima");

        var fechaLocal = formulario.FechaEntregaProgramada
            ?? throw new ValidationException(
                "Selecciona la fecha de entrega.");

        var fechaEntregaUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified),
            zonaLima);

        if (fechaEntregaUtc <= DateTime.UtcNow)
        {
            throw new ValidationException(
                "La fecha y hora de entrega deben ser futuras, en hora de Lima.");
        }

        await using var transaccion =
            await _contexto.Database.BeginTransactionAsync(
                cancellationToken);

        var actualizados = await _contexto.Productos
            .Where(producto => producto.Id == formulario.ProductoId
                && producto.Activo
                && producto.StockFisico - producto.StockReservado
                    >= formulario.Cantidad)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(
                producto => producto.StockReservado,
                producto => producto.StockReservado + formulario.Cantidad),
                cancellationToken);

        if (actualizados != 1)
        {
            throw new ValidationException(
                "El producto no está disponible o la cantidad supera las existencias.");
        }

        var producto = await _contexto.Productos
            .AsNoTracking()
            .SingleAsync(
                p => p.Id == formulario.ProductoId,
                cancellationToken);

        var codigo = "AF-" + Convert.ToHexString(
            RandomNumberGenerator.GetBytes(12));

        var pedido = new Pedido
        {
            Codigo = codigo,
            Estado = "Pendiente",
            FechaRegistro = DateTime.UtcNow,
            FechaEntregaProgramada = fechaEntregaUtc,
            ModalidadEntrega = formulario.ModalidadEntrega,
            DireccionEntrega = formulario.ModalidadEntrega == "Delivery"
                ? formulario.DireccionEntrega!.Trim()
                : null,

            Cliente = new Cliente
            {
                Nombre = formulario.Nombre.Trim(),
                Telefono = formulario.Telefono.TrimStart('+'),
                Correo = string.IsNullOrWhiteSpace(formulario.Correo)
                    ? null
                    : formulario.Correo.Trim()
            },

            Detalles = new List<DetallePedido>
            {
                new DetallePedido
                {
                    ProductoId = producto.Id,
                    Cantidad = formulario.Cantidad,
                    PrecioUnitario = producto.Precio
                }
            }
        };

        _contexto.Pedidos.Add(pedido);

        await _contexto.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);

        return codigo;
    }
}