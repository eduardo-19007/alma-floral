USE AlmaFloralDb;
GO
-- Consulta de solo lectura: pedidos y detalle, sin datos personales.
SELECT p.Codigo, p.Estado, p.ModalidadEntrega,
       pr.Nombre AS Producto, d.Cantidad, d.PrecioUnitario,
       d.Cantidad * d.PrecioUnitario AS Subtotal,
       p.FechaEntregaProgramada AS FechaEntregaUTC
FROM dbo.Pedido AS p
JOIN dbo.DetallePedido AS d ON d.PedidoId = p.PedidoId
JOIN dbo.Producto AS pr ON pr.ProductoId = d.ProductoId
ORDER BY p.PedidoId DESC;

-- La disponibilidad se calcula descontando las reservas del stock físico.
SELECT ProductoId, Nombre, StockFisico, StockReservado,
       StockFisico - StockReservado AS StockDisponible
FROM dbo.Producto
ORDER BY ProductoId;
SELECT CasoPostventaId, PedidoId, UsuarioId, Tipo, Estado, FechaApertura, FechaCierre, Respuesta, Acuerdo FROM dbo.CasoPostventa ORDER BY CasoPostventaId;
