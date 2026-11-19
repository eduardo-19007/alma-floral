# Diccionario de datos — Alma Floral

Diseño propuesto para SQL Server.
PK significa clave primaria; FK, clave foránea;
UK, valor único. Los identificadores PK serán IDENTITY(1,1).

## CLIENTE

| Campo | Tipo | Obligatorio | Descripción |
|---|---|---|---|
| ClienteId | int, PK | Sí | Identificador del cliente. |
| Nombre | nvarchar(150) | Sí | Nombre del cliente. |
| Telefono | nvarchar(20) | Sí | Número de contacto. |
| Correo | nvarchar(254) | No | Correo de contacto. |

## PRODUCTO

| Campo | Tipo | Obligatorio | Descripción |
|---|---|---|---|
| ProductoId | int, PK | Sí | Identificador del producto. |
| Nombre | nvarchar(120) | Sí | Nombre comercial. |
| Descripcion | nvarchar(500) | No | Características del producto. |
| Precio | decimal(10,2) | Sí | Precio vigente en soles. |
| StockFisico | int | Sí | Unidades existentes. Valor inicial: 0. |
| StockReservado | int | Sí | Unidades comprometidas en pedidos. Valor inicial: 0. |
| Activo | bit | Sí | Indica si el producto está habilitado. Valor inicial: 1. |

El precio y ambos valores de stock deben ser mayores o iguales a cero.
StockReservado no puede superar a StockFisico.
El stock disponible es StockFisico menos StockReservado.

## PEDIDO

| Campo | Tipo | Obligatorio | Descripción |
|---|---|---|---|
| PedidoId | int, PK | Sí | Identificador interno del pedido. |
| ClienteId | int, FK | Sí | Referencia a CLIENTE. |
| Codigo | nvarchar(30), UK | Sí | Código único para identificar el pedido. |
| FechaRegistro | datetime2(0) | Sí | Fecha y hora de registro. |
| Estado | nvarchar(20) | Sí | Estado del pedido. Inicial: Registrado. |
| ModalidadEntrega | nvarchar(10) | Sí | Delivery o Recojo. |
| DireccionEntrega | nvarchar(300) | Condicional | Obligatoria para Delivery; puede ser nula para Recojo. |
| FechaEntregaProgramada | datetime2(0) | Sí | Fecha y hora acordadas para la entrega o recojo. |

Estados permitidos: Registrado, En preparación, Listo,
Entregado y Cancelado.

La aplicación debe validar las transiciones de estado
y evitar reservar, liberar o descontar stock más de una vez.

## DETALLE_PEDIDO

| Campo | Tipo | Obligatorio | Descripción |
|---|---|---|---|
| DetallePedidoId | int, PK | Sí | Identificador de la línea. |
| PedidoId | int, FK | Sí | Referencia a PEDIDO. |
| ProductoId | int, FK | Sí | Referencia a PRODUCTO. |
| Cantidad | int | Sí | Unidades solicitadas; debe ser mayor que cero. |
| PrecioUnitario | decimal(10,2) | Sí | Precio acordado en soles; no puede ser negativo. |

Cada producto aparece una sola vez por pedido,
mediante una restricción única sobre PedidoId y ProductoId.

El subtotal se calcula como Cantidad por PrecioUnitario.
El total del pedido es la suma de sus subtotales.
Un pedido debe registrarse con al menos un detalle;
la aplicación debe garantizarlo dentro de una transacción.

## USUARIO

| Campo | Tipo | Obligatorio | Descripción |
|---|---|---|---|
| UsuarioId | int, PK | Sí | Identificador del usuario interno. |
| Nombre | nvarchar(150) | Sí | Nombre del responsable. |
| Correo | nvarchar(254), UK | Sí | Correo único para el acceso. |
| HashContrasena | nvarchar(max) | Sí | Hash generado por el mecanismo de autenticación. |
| Rol | nvarchar(30) | Sí | Administrador o Atencion. |
| Activo | bit | Sí | Indica si el acceso está habilitado. Valor inicial: 1. |

El rol Administrador permite gestionar el catálogo.
El rol Atencion permite gestionar pedidos y postventa.
Los permisos deben comprobarse en la aplicación.

## CASO_POSTVENTA

| Campo | Tipo | Obligatorio | Descripción |
|---|---|---|---|
| CasoPostventaId | int, PK | Sí | Identificador del caso. |
| PedidoId | int, FK | Sí | Referencia al pedido relacionado. |
| UsuarioId | int, FK | No | Responsable asignado; puede ser nulo inicialmente. |
| Tipo | nvarchar(30) | Sí | Consulta, Reclamo o Solicitud. |
| Descripcion | nvarchar(2000) | Sí | Motivo comunicado por el cliente. |
| Estado | nvarchar(20) | Sí | Abierto, En atención o Cerrado. Inicial: Abierto. |
| FechaApertura | datetime2(0) | Sí | Fecha y hora de creación. |
| FechaCierre | datetime2(0) | Condicional | Obligatoria al cerrar el caso. |
| Respuesta | nvarchar(2000) | Condicional | Obligatoria al cerrar el caso. |

La fecha de cierre no puede ser anterior a la apertura.
Un caso en atención o cerrado debe tener un responsable asignado.

## Conservación e integridad

- Las claves foráneas deben impedir referencias inexistentes.
- No se utilizará borrado en cascada para eliminar el historial.
- Los productos y usuarios se desactivarán cuando corresponda.
- Los campos de texto obligatorios no deben aceptar contenido vacío.
- Las reservas y salidas de stock deben actualizarse mediante transacciones.
- Las fechas seguirán una convención uniforme documentada al implementar.
- Este diccionario describe el diseño; todavía no acredita una base de datos implementada.
